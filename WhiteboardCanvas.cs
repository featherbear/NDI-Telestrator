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

        public List<InkLayer> InkLayers
        {
            get
            {
                List<InkLayer> result = new List<InkLayer>();
                foreach (InkLayer canvas in Children)
                {
                    result.Add(canvas);
                }
                return result;
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

            addNewLayer();
        }

        private void _notifyUpdate(InkLayer layer = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkLayers"));
            CanvasUpdated?.Invoke(this, layer);
        }

        public void addNewLayer()
        {
            InkLayer layer = new InkLayer(this);

            layer.LayerUpdated += (_, __) =>
            {
                updateUndoRedoStates();
                _notifyUpdate(layer);
            };

            this.Children.Add(layer);
            setActive(layer);

            _notifyUpdate();
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

        public System.Windows.Media.Imaging.BitmapFrame Draw(Brush background = null)
        {
            return InkControls.Draw(InkLayers.Select(c => c.Strokes).ToArray(), background);
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
            hasUndoContent = activeInkCanvas.backHistory.Count > 0;
            hasRedoContent = activeInkCanvas.forwardHistory.Count > 0;
        }

        public void Clear()
        {
            activeInkCanvas.Strokes.Clear();
        }

        public void setActive(InkLayer layer)
        {
            if (!Children.Contains(layer)) throw new Exception("Could not find requested layer in canvas");

            foreach (InkLayer l in Children) l.IsHitTestVisible = l == layer;
            activeInkCanvas = layer;
        }

        public void setActive(int index)
        {
            if (index >= Children.Count) throw new IndexOutOfRangeException("Got index " + index + " but max index is " + (Children.Count - 1));

            for (int i = 0; i < Children.Count; i++) Children[i].IsHitTestVisible = i == index;
            activeInkCanvas = (InkLayer)Children[index];
        }
    }
}
