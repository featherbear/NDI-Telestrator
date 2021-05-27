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


    // TODO: TURN BACK TO Canvas
    public class WhiteboardCanvas : System.Windows.Controls.Grid, INotifyPropertyChanged
    {
        internal class CanvasData
        {
            public CanvasData()
            {
                redoQueue = new Queue<Stroke>();
            }
            public Queue<Stroke> redoQueue;
        };

        private double brushThickness = 1.0;
        private Color brushColor = Colors.Black;

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
                updateUndoRedoStates();
            }
        }

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

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;







        public WhiteboardCanvas()
        {
            InitializeComponent();

        }
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
                Console.WriteLine("lost capture" + activeInkCanvas.Strokes.Count);
            };
            
            activeInkCanvas.PreviewStylusDown += (a, b) =>
            {
                activeInkCanvas.Strokes.Add(new Stroke(stylusStrokeBuffer = b.StylusDevice.GetStylusPoints(activeInkCanvas)));
                b.Handled = true;
            };

            // Cancel mouse down event if the stylus was used (Prevents single pixel stroke)
            activeInkCanvas.PreviewMouseDown += (a, b) => { if (stylusStrokeBuffer != null) b.Handled = true; };

            activeInkCanvas.PreviewStylusUp += (a, b) =>
             {
                 // Clear the buffer when the stylus is lifted
                 stylusStrokeBuffer = null;

                 // Manually trigger the stroke collected event
                 activeInkCanvas.RaiseEvent(new InkCanvasStrokeCollectedEventArgs(activeInkCanvas.Strokes[activeInkCanvas.Strokes.Count - 1]) { RoutedEvent = InkCanvas.StrokeCollectedEvent });
             };

            activeInkCanvas.PreviewStylusMove += (a, b) =>
            {
                // Add points to the buffer
                stylusStrokeBuffer.Add(b.StylusDevice.GetStylusPoints(activeInkCanvas));

                // This override blocks the delayed ink from being added to the point
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
