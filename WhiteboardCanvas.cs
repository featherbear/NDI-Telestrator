using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Veldrid;

namespace NDI_Telestrator
{

    public class WhiteboardCanvas : System.Windows.Controls.Canvas, INotifyPropertyChanged


    {

        private GraphicsDevice _gd;
        private Swapchain _sc;
        private CommandList _cl;

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


        private void InitializeComponent()
        {
            this.Background = System.Windows.Media.Brushes.Transparent;
            SizeChanged += WhiteboardCanvas_SizeChanged;

            addNewLayer();

            LostMouseCapture += (a, b) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InkCanvases2"));

            }
            ;
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


            //_gd = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());

            //Window window = Window.GetWindow(this);
            //IntPtr w = new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle();
            //SwapchainSource source = SwapchainSource.CreateWin32(w, Marshal.GetHINSTANCE(typeof(WhiteboardCanvas).Module));

            //_sc = _gd.ResourceFactory.CreateSwapchain(new SwapchainDescription(source, (uint)Width, (uint)Height, null, false));
            //_cl = _gd.ResourceFactory.CreateCommandList();
            //Console.WriteLine("Lol");

            //_cl.Begin();
            //_cl.SetFramebuffer(_sc.Framebuffer);
            //_cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            //_cl.End();

            //_gd.SubmitCommands(_cl);
            //_cl.CopyTexture()
            //_gd.SwapBuffers(_sc);

            ]

               var rf = new Veldrid.Utilities.DisposeCollectorResourceFactory(_gd.ResourceFactory);

            var colorTargetTexture = _gd.SwapchainFramebuffer.ColorTargets[0].Target;
            var pixelFormat = colorTargetTexture.Format; // <- PixelFormat.B8_G8_R8_A8_UNorm, is it OK?

            var textureDescription = colorTargetTexture.GetDescription();
            textureDescription.Usage = TextureUsage.RenderTarget;
            textureDescription.Type = TextureType.Texture2D;
            textureDescription.Format = pixelFormat;

            var textureForRender = rf.CreateTexture(textureDescription);

            //var depthTexture = _gd.SwapchainFramebuffer.DepthTarget.Value.Target;
            //var depthTextureForRender = rf.CreateTexture(depthTexture.GetDescription());

            var framebufferDescription = new FramebufferDescription(null, textureForRender);
            var framebuffer = rf.CreateFramebuffer(framebufferDescription);

            Texture stage = rf.CreateTexture(TextureDescription.Texture2D(
                textureForRender.Width,
                textureForRender.Height,
                1,
                1,
                pixelFormat,
                TextureUsage.Staging));

            SubmitUI();

            _cl.Begin();
            _cl.SetFramebuffer(framebuffer);

            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);

            _cl.CopyTexture(
                textureForRender, 0, 0, 0, 0, 0,
                stage, 0, 0, 0, 0, 0,
                stage.Width, stage.Height, 1, 1);
            _cl.End();

            _gd.SubmitCommands(_cl);

            MappedResourceView<Rgba32> map = _gd.Map<Rgba32>(stage, MapMode.Read);

            var image = new Image<Rgba32>((int)stage.Width, (int)stage.Height);

            Rgba32[] pixelData = new Rgba32[stage.Width * stage.Height];
            for (int y = 0; y < stage.Height; y++)
            {
                for (int x = 0; x < stage.Width; x++)
                {
                    //int index = (int)(y * stage.Width + x);
                    //pixelData[index] = map[x, y]; // <- I have to convert BGRA to RGBA pixels here
                    image[x, y] = new Rgba32(map[x, y].B, map[x, y].G, map[x, y].R, map[x, y].A);
                }
            }

            _gd.Unmap(stage);
            rf.DisposeCollector.DisposeAll();

            image.SaveAsPng("test.png");
        }

    }
}
