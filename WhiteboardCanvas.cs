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

        public event EventHandler<InkLayer> CanvasUpdated;

        private double brushThickness = 1.0;
        private Color brushColor = Colors.Black;

        #region activeInkCanvas
        private InkLayer _activeInkCanvas;
        public InkLayer activeInkCanvas
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

        private void InitializeComponent()
        {
            this.Background = System.Windows.Media.Brushes.Transparent;

            SizeChanged += WhiteboardCanvas_SizeChanged;

            addNewLayer();
        }

        public void addNewLayer()
        {
            InkLayer canvas = new InkLayer(this);
            canvas.LayerUpdated += (layer, _) =>
            {
                updateUndoRedoStates();
                CanvasUpdated?.Invoke(this, (InkLayer) layer);
            };
            this.Children.Add(canvas);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases2"));

            activeInkCanvas = canvas;
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
                activeInkCanvas.Undo();
        }

        public void Redo()
        {
            activeInkCanvas.Redo();
        }

        public void updateUndoRedoStates()
        {
            hasUndoContent = activeInkCanvas.Strokes.Count > 0;
            hasRedoContent = activeInkCanvas.redoQueue.Count > 0;
        }

        public void Clear()
        {
            activeInkCanvas.Strokes.Clear();
        }

    }
}
