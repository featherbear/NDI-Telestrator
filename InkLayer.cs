using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NDI_Telestrator
{
    public class InkLayer : System.Windows.Controls.InkCanvas
    {

        #region Property notifications
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public enum HistoryDataType
        {
            ClearBoard,
            AddStroke,
            DeleteStroke
        }

        public struct HistoryData
        {
            public HistoryDataType type;
            public object dataA;
            public object dataB;
        }


        public Stack<HistoryData> backHistory = new Stack<HistoryData>();
        public Queue<HistoryData> forwardHistory = new Queue<HistoryData>();

        /// <summary>
        /// Use this function when adding a new item to the back history.
        /// EXCEPT for Undo / Redo operations
        /// </summary>
        /// <param name="evt">History Data</param>
        private void pushBackHistory(HistoryData evt)
        {
            // TODO: Check if working wtih stroke move / copy / drag

            backHistory.Push(evt);
            forwardHistory.Clear();
            _notifyUpdate();
        }



        public event EventHandler LayerUpdated;

        // The stylus/touch ink doesn't get captured via the RenderTargetBitmap
        // function so we'll add it to a Stroke that we add it in
        StylusPointCollection stylusStrokeBuffer = null;

        public BitmapFrame Bitmap
        {
            get
            {
                return Draw(Brushes.White);
            }
        }

        private void _notifyUpdate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bitmap"));
            LayerUpdated?.Invoke(this, null);
        }

        public InkLayer(Canvas parent) : base()
        {
            Background = System.Windows.Media.Brushes.Transparent;
            UseCustomCursor = true;

            Width = parent.Width;
            Height = parent.Height;

            parent.SizeChanged += (a, b) =>
            {
                Width = b.NewSize.Width;
                Height = b.NewSize.Height;
                _notifyUpdate();
            };
        }

        // Generate a bitmap of the individual layer
        public BitmapFrame Draw(Brush background = null)
        {
            if (double.IsNaN(Width)) return null;
            //if (!IsLoaded) return null;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                if (background != null) drawingContext.DrawRectangle(background, null, new Rect(0, 0, (int)Width, (int)Height));
                Strokes.Draw(drawingContext);
                drawingContext.Close();

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)Width, (int)Height, 96d, 96d, PixelFormats.Default);
                rtb.Render(drawingVisual);

                return BitmapFrame.Create(rtb);
            }
        }

        public void Undo()
        {
            if (backHistory.Count > 0)
            {
                HistoryData evt = backHistory.Pop();

                switch (evt.type)
                {
                    case HistoryDataType.AddStroke:
                        Strokes.Remove((Stroke)evt.dataA); //Strokes.RemoveAt(Strokes.Count - 1);
                        break;
                    case HistoryDataType.ClearBoard:
                        evt.dataB = Strokes;
                        Strokes = (StrokeCollection)evt.dataA;
                        break;
                }

                forwardHistory.Enqueue(evt);
                _notifyUpdate();
            }
        }

        public void Redo()
        {
            if (forwardHistory.Count > 0)
            {
                HistoryData evt = forwardHistory.Dequeue();

                switch (evt.type)
                {
                    case HistoryDataType.AddStroke:
                        Strokes.Add((Stroke)evt.dataA);
                        break;

                    case HistoryDataType.ClearBoard:
                        evt.dataA = Strokes;
                        Strokes = (StrokeCollection)evt.dataB;
                        break;
                }

                backHistory.Push(evt);
                _notifyUpdate();
            }
        }



        public void Clear()
        {
            pushBackHistory(new HistoryData { type = HistoryDataType.ClearBoard, dataA = Strokes });
            Strokes = new StrokeCollection();
            _notifyUpdate();
        }

        private void _handleStrokeCollection(Stroke stroke)
        {
            pushBackHistory(new HistoryData { type = HistoryDataType.AddStroke, dataA = stroke });
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Do nothing
            // Don't let anyone use this uwu
            // base.OnStrokeCollected(e);
        }

        protected override void OnPreviewStylusDown(StylusDownEventArgs e)
        {
            Strokes.Add(new Stroke(stylusStrokeBuffer = e.StylusDevice.GetStylusPoints(this), DefaultDrawingAttributes));

            // Handling here slows the rendering
            // But not handling adds a 1-pixel (or so) stroke
            // EDIT: If PreviewStylusUp is handled, the 1-pixel stroke is not added
            // b.Handled = true;

            base.OnPreviewStylusDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            // Cancel mouse down event if the stylus was used (Prevents single pixel stroke)
            if (stylusStrokeBuffer != null)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewMouseDown(e);
        }

        //protected override void OnPreviewStylusUp(StylusEventArgs e)
        //{
        //    // Clear the buffer when the stylus is lifted
        //    //     // stylusStrokeBuffer = null;

        //    //     // Manually trigger events
        //    //     // RaiseEvent(new InkCanvasStrokeCollectedEventArgs(Strokes[Strokes.Count - 1]) { RoutedEvent = InkCanvas.StrokeCollectedEvent });

        //    //     // Blocking this event stops the 1-pixel stroke from being added
        //    //     // But currently breaks mouse use (need to release the stylus / mouse?)
        //    //     // EDIT: For now we'll just remove it I guess..
        //    //     // e.Handled = true;
        //    base.OnPreviewStylusUp(e);
        //}

        // The 1-pixel stroke gets added somewhere between PreviewStylusUp and StylusUp

        private bool wasStrokeCaptured = false;

        protected override void OnStylusUp(StylusEventArgs e)
        {
            if (stylusStrokeBuffer != null)
            {
                // Remove the last stroke (1)
                stylusStrokeBuffer = null;
                Strokes.RemoveAt(Strokes.Count - 1);
                wasStrokeCaptured = true;

                // Using OnMouseLeftButtonUp instead now
                // OnStrokeCollected(new InkCanvasStrokeCollectedEventArgs(Strokes[Strokes.Count - 1]) { RoutedEvent = InkCanvas.StrokeCollectedEvent });
            }

            base.OnStylusUp(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (!wasStrokeCaptured) wasStrokeCaptured = IsMouseCaptured;
            base.OnPreviewMouseUp(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (wasStrokeCaptured) _handleStrokeCollection(Strokes[Strokes.Count - 1]);

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnPreviewStylusMove(StylusEventArgs e)
        {
            if (stylusStrokeBuffer == null)
            {
                base.OnPreviewStylusMove(e);
                return;
            }

            // Add points to the buffer
            stylusStrokeBuffer.Add(e.StylusDevice.GetStylusPoints(this));

            // Blocks events that would populate the 1-pixel stroke
            e.Handled = true;
        }
    }
}
