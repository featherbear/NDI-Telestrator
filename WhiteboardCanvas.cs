using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Media;

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

        private double brushThickness = 2.0;
        private Color brushColour = Colors.Black;

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
                _notifyUpdate(_activeInkCanvas);
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
        public bool hasRedoContent
        {
            get
            {
                return activeInkCanvas.forwardHistory.Count > 0;
            }
        }
        #endregion

        #region hasUndoContent
        public bool hasUndoContent
        {
            get
            {
                return activeInkCanvas.backHistory.Count > 0;
            }
        }
        #endregion


        #region hasStrokes
        public bool hasStrokes
        {
            get
            {
                return activeInkCanvas.Strokes.Count > 0;
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

        public void ResetState(bool addLayer = true)
        {
            this.Children.Clear();
            if (addLayer) addNewLayer();
        }


        private void _notifyUpdate(InkLayer layer = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("hasUndoContent"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("hasRedoContent"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("hasStrokes"));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkLayers"));
            CanvasUpdated?.Invoke(this, layer);
        }

        public void addNewLayer(StrokeCollection strokes = null)
        {
            InkLayer layer = new InkLayer(this);
            if (strokes != null) layer.Strokes = strokes;

            layer.LayerUpdated += (_, __) =>
            {
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


        public void SetPenColour(Color colour)
        {
            brushColour = colour;
            setPenAttributes(brushColour, brushThickness);
        }

        public void SetPenColor(Brush colour)
        {
            SetPenColour(((SolidColorBrush)colour).Color);
        }

        public void SetPenThickness(double size)
        {
            setPenAttributes(brushColour, (brushThickness = size));
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

        public void Clear()
        {
            activeInkCanvas.Clear();
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
