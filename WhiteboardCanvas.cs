using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NDI_Telestrator
{

    public class WhiteboardCanvas : System.Windows.Controls.Canvas, INotifyPropertyChanged
    {

        #region Property notifications
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region CanvasData
        internal class CanvasData
        {
            public CanvasData()
            {
                redoQueue = new Queue<Stroke>();
            }
            public Queue<Stroke> redoQueue;
        };
        #endregion

        private double brushThickness = 1.0;
        private Color brushColor = Colors.Black;

        #region activeInkCanvas
        private InkCanvas _activeInkCanvas;
        public InkCanvas activeInkCanvas
        {
            get
            {
                return _activeInkCanvas;
            }
            set
            {
                _activeInkCanvas = value;
                OnPropertyChanged();
                stylusStrokeBuffer = null;
                updateUndoRedoStates();
            }
        }
        #endregion

        public List<InkCanvas> InkCanvases
        {
            get
            {
                List<InkCanvas> result = new List<InkCanvas>();
                foreach (InkCanvas canvas in Children)
                {
                    result.Add(canvas);
                }
                return result;
            }
        }

        private List<int> _inkCanveses2;
        public List<int> InkCanvases2
        {
            get
            {
                List<InkCanvas> result = new List<InkCanvas>();
                foreach (InkCanvas canvas in Children)
                {
                    result.Add(canvas);

                }
                return result.Select(e => e.Strokes.Count).ToList();
            }
        }

        #region hasRedoContent
        private bool _hasRedoContent;
        public bool hasRedoContent
        {
            get
            {
                return _hasRedoContent;
            }
            private set
            {
                _hasRedoContent = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region hasUndoContent
        private bool _hasUndoContent;
        public bool hasUndoContent
        {
            get
            {
                return _hasUndoContent;
            }
            private set
            {
                _hasUndoContent = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public WhiteboardCanvas()
        {
            InitializeComponent();
        }

        // The stylus/touch ink doesn't get captured via the RenderTargetBitmap
        // function so we'll add it to a Stroke that we add it in
        StylusPointCollection stylusStrokeBuffer;

        private void InitializeComponent()
        {
            this.Background = System.Windows.Media.Brushes.Transparent;

            SizeChanged += WhiteboardCanvas_SizeChanged;

            addNewLayer();
            MouseDevice mouseDev = InputManager.Current.PrimaryMouseDevice;

            activeInkCanvas.LostMouseCapture += (a, b) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases2"));
            };

            activeInkCanvas.PreviewStylusDown += (a, b) =>
            {
                activeInkCanvas.Strokes.Add(new Stroke(stylusStrokeBuffer = b.StylusDevice.GetStylusPoints(activeInkCanvas), activeInkCanvas.DefaultDrawingAttributes));

                // Handling here slows the rendering
                // But not handling adds a 1-pixel (or so) stroke
                // EDIT: If PreviewStylusUp is handled, the 1-pixel stroke is not added
                // b.Handled = true;
            };

            // Cancel mouse down event if the stylus was used (Prevents single pixel stroke)
            activeInkCanvas.PreviewMouseDown += (a, b) => { if (stylusStrokeBuffer != null) b.Handled = true; };

            //activeInkCanvas.PreviewStylusUp += (a, b) =>
            // {
            //     // Clear the buffer when the stylus is lifted
            //     // stylusStrokeBuffer = null;

            //     // Manually trigger events
            //     // activeInkCanvas.RaiseEvent(new InkCanvasStrokeCollectedEventArgs(activeInkCanvas.Strokes[activeInkCanvas.Strokes.Count - 1]) { RoutedEvent = InkCanvas.StrokeCollectedEvent });

            //     // Blocking this event stops the 1-pixel stroke from being added
            //     // But currently breaks mouse use (need to release the stylus / mouse?)
            //     // EDIT: For now we'll just remove it I guess..
            //     // b.Handled = true;
            // };

            // The 1-pixel stroke gets added somewhere between PreviewStylusUp and StylusUp

            activeInkCanvas.StylusUp += (a, b) =>
            {
                // Remove the last stroke (1)
                stylusStrokeBuffer = null;
                activeInkCanvas.Strokes.RemoveAt(activeInkCanvas.Strokes.Count - 1);

                activeInkCanvas.RaiseEvent(new InkCanvasStrokeCollectedEventArgs(activeInkCanvas.Strokes[activeInkCanvas.Strokes.Count - 1]) { RoutedEvent = InkCanvas.StrokeCollectedEvent });
            };

            activeInkCanvas.PreviewStylusMove += (a, b) =>
            {
                // Add points to the buffer
                stylusStrokeBuffer.Add(b.StylusDevice.GetStylusPoints(activeInkCanvas));

                // Blocks events that would populate the 1-pixel stroke
                b.Handled = true;
            };
        }

        public void addNewLayer()
        {
            InkCanvas canvas = CreateLayer();
            this.Children.Add(canvas);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases2"));

            activeInkCanvas = canvas;
        }
        private InkCanvas CreateLayer()
        {
            InkCanvas canvas = new InkCanvas();
            // Clear the redo queue on new stroke input
            // TODO: Check if working wtih stroke move / copy / drag
            canvas.Tag = new CanvasData();

            canvas.StrokeCollected += (sender, args) =>
            {
                ((CanvasData)canvas.Tag).redoQueue.Clear();
                updateUndoRedoStates();
            };
            canvas.Background = System.Windows.Media.Brushes.Transparent;
            canvas.UseCustomCursor = true;
            canvas.Cursor = this.Cursor;
            canvas.Width = this.Width;
            canvas.Height = this.Height;
            return canvas;
        }


        private void WhiteboardCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            foreach (InkCanvas canvas in this.Children)
            {
                canvas.Width = this.Width;
                canvas.Height = this.Height;
            }
        }

        private void setPenAttributes(Color color, double size)
        {
            DrawingAttributes inkDA = new DrawingAttributes();
            inkDA.Width = size;
            inkDA.Height = size;
            inkDA.Color = color;
            // inkDA.FitToCurve = true;
            //inkDA.StylusTip = StylusTip.Rectangle;
            //inkDA.IsHighlighter = true;
            // inkDA.IgnorePressure

            activeInkCanvas.DefaultDrawingAttributes = inkDA;
            //__setPenAttributes(color, size);
        }


        //private void __setPenAttributes(Color color, double size)
        //{
        //    // DrawingAttributes inkDA = new DrawingAttributes();
        //    // inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
        //    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;

        //    //inkCanvas.DefaultDrawingAttributes = inkDA;



        //    inkCanvas.Select(new StrokeCollection());
        //}


        public void SetPenColor(Color color)
        {
            brushColor = color;
            setPenAttributes(brushColor, brushThickness);
        }

        public void SetPenColor(Brush color)
        {
            var scb = (SolidColorBrush)color;
            SetPenColor(scb.Color);
        }

        public void SetPenThickness(double size)
        {
            brushThickness = size;
            setPenAttributes(brushColor, size);
        }


        public void Undo()
        {
            if (activeInkCanvas.Strokes.Count > 0)
            {
                ((CanvasData)activeInkCanvas.Tag).redoQueue.Enqueue(activeInkCanvas.Strokes.Last());
                activeInkCanvas.Strokes.RemoveAt(activeInkCanvas.Strokes.Count - 1);

                updateUndoRedoStates();
            }
        }

        public void Redo()
        {
            if (((CanvasData)activeInkCanvas.Tag).redoQueue.Count > 0)
            {
                activeInkCanvas.Strokes.Add(((CanvasData)activeInkCanvas.Tag).redoQueue.Dequeue());
                updateUndoRedoStates();
            }
        }

        public void updateUndoRedoStates()
        {
            hasUndoContent = activeInkCanvas.Strokes.Count > 0;
            hasRedoContent = ((CanvasData)activeInkCanvas.Tag).redoQueue.Count > 0;
        }

        public void Clear()
        {
            activeInkCanvas.Strokes.Clear();
        }

    }
}
