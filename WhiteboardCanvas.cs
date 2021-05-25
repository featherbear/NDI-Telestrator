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

        private double brushThickness = 1.0;
        private Color brushColor = Colors.Black;
        public InkCanvas inkCanvas;

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


        private Queue<Stroke> redoQueue = new Queue<Stroke>();


        public WhiteboardCanvas()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            inkCanvas = new InkCanvas();

            // Clear the redo queue on new stroke input
            // TODO: Check if working wtih stroke move / copy / drag
            inkCanvas.StrokeCollected += (sender, args) =>
            {
                redoQueue.Clear();
                updateUndoRedoStates();
            };


            this.Background = System.Windows.Media.Brushes.Transparent;
            inkCanvas.Background = System.Windows.Media.Brushes.Transparent;

            SizeChanged += WhiteboardCanvas_SizeChanged;

            inkCanvas.UseCustomCursor = true;
            inkCanvas.Cursor = this.Cursor;

            this.Children.Add(inkCanvas);
        }

        private void WhiteboardCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // make sure the ink canvas also changes
            inkCanvas.Width = this.Width;
            inkCanvas.Height = this.Height;
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

            inkCanvas.DefaultDrawingAttributes = inkDA;
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

        private void TODO()
        {
            // Layers
        }



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
            if (inkCanvas.Strokes.Count > 0)
            {
                redoQueue.Enqueue(inkCanvas.Strokes.Last());
                inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count - 1);

                updateUndoRedoStates();
            }
        }

        public void Redo()
        {
            if (redoQueue.Count > 0)
            {
                inkCanvas.Strokes.Add(redoQueue.Dequeue());
                updateUndoRedoStates();
            }
        }

        public void updateUndoRedoStates()
        {
            hasUndoContent = inkCanvas.Strokes.Count > 0;
            hasRedoContent = redoQueue.Count > 0;
        }

        public void Clear()
        {
            inkCanvas.Strokes.Clear();
        }

    }
}
