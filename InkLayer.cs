using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace NDI_Telestrator
{
    public class InkLayer : System.Windows.Controls.InkCanvas
    {

        public event EventHandler LayerUpdated;

        public Queue<Stroke> redoQueue;

        // The stylus/touch ink doesn't get captured via the RenderTargetBitmap
        // function so we'll add it to a Stroke that we add it in
        StylusPointCollection stylusStrokeBuffer = null;

        public InkLayer(Canvas parent)
        {
            Background = System.Windows.Media.Brushes.Transparent;
            UseCustomCursor = true;
            Cursor = parent.Cursor;
            Width = parent.Width;
            Height = parent.Height;
            redoQueue = new Queue<Stroke>();
        }

        public void Undo()
        {
            if (Strokes.Count > 0)
            {
                redoQueue.Enqueue(Strokes.Last());
                Strokes.RemoveAt(Strokes.Count - 1);

                LayerUpdated?.Invoke(this, null);
            }
        }

        public void Redo()
        {
            if (redoQueue.Count > 0)
            {
                Strokes.Add(redoQueue.Dequeue());

                LayerUpdated?.Invoke(this, null);
            }
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Clear the redo queue on new stroke input
            // TODO: Check if working wtih stroke move / copy / drag
            redoQueue.Clear();
            LayerUpdated?.Invoke(this, null);
            base.OnStrokeCollected(e);
        }
        //protected override void OnLostMouseCapture(MouseEventArgs e)
        //{
        //    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases2"));
        //    base.OnLostMouseCapture(e);
        //}

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
            if (stylusStrokeBuffer != null) e.Handled = true;
            else base.OnPreviewMouseDown(e);
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

        protected override void OnPreviewStylusUp(StylusEventArgs e)
        {
            // Remove the last stroke (1)
            stylusStrokeBuffer = null;
            Strokes.RemoveAt(Strokes.Count - 1);

            OnStrokeCollected(new InkCanvasStrokeCollectedEventArgs(Strokes[Strokes.Count - 1]) { RoutedEvent = InkCanvas.StrokeCollectedEvent });
            base.OnPreviewStylusUp(e);
        }

        protected override void OnPreviewStylusMove(StylusEventArgs e)
        {
            // Add points to the buffer
            stylusStrokeBuffer.Add(e.StylusDevice.GetStylusPoints(this));

            // Blocks events that would populate the 1-pixel stroke
            e.Handled = true;
            //base.OnPreviewStylusMove(e);
        }
    }
}
