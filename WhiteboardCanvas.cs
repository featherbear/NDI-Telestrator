
using SixLabors.ImageSharp.Formats.Png;
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
           // activeInkCanvas.Strokes.Clear();


            _gd = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());

            Window window = Window.GetWindow(this);
            IntPtr w = new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle();
            SwapchainSource source = SwapchainSource.CreateWin32(w, Marshal.GetHINSTANCE(typeof(WhiteboardCanvas).Module));
            Console.WriteLine("intptr" +  Marshal.GetHINSTANCE(typeof(WhiteboardCanvas).Module));

            
            
            _sc = _gd.ResourceFactory.CreateSwapchain(new SwapchainDescription(source, (uint)Width, (uint)Height, null, false));
            Console.WriteLine(_sc.Framebuffer.ColorTargets.Count);

            var a = _sc.Framebuffer.ColorTargets[0].Target;
            _cl = _gd.ResourceFactory.CreateCommandList();

            Texture renderTexture = _gd.ResourceFactory.CreateTexture(new TextureDescription(
                a.Width, a.Height, a.Depth, a.MipLevels, a.ArrayLayers, a.Format, TextureUsage.RenderTarget, TextureType.Texture2D, a.SampleCount
                ));

            var framebufferDescription = new FramebufferDescription(null, renderTexture);
            var framebuffer = _gd.ResourceFactory.CreateFramebuffer(framebufferDescription);

            Texture stage = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
             renderTexture.Width,
             renderTexture.Height,
             1,
             1,
             a.Format,
             TextureUsage.Staging));

            _cl.Begin();
            _cl.SetFramebuffer(_sc.Framebuffer);
            //_cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);


          
            //Console.WriteLine("screenG" + screenG);
            try
            {
                Console.WriteLine(ActualWidth);
                
            } catch
            {
                
            }

            _cl.CopyTexture(
             renderTexture, 0, 0, 0, 0, 0,
             stage, 0, 0, 0, 0, 0,
             stage.Width, stage.Height, 1, 1);
            _cl.End();

            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_sc);
            Console.WriteLine(_gd.MainSwapchain);

            MappedResourceView<SixLabors.ImageSharp.PixelFormats.Rgba32 > map = _gd.Map<SixLabors.ImageSharp.PixelFormats.Rgba32>(stage, MapMode.Read);

            var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>((int)stage.Width, (int)stage.Height);

            SixLabors.ImageSharp.PixelFormats.Rgba32[] pixelData = new SixLabors.ImageSharp.PixelFormats.Rgba32[stage.Width * stage.Height];
            for (int y = 0; y < stage.Height; y++)
            {
                for (int x = 0; x < stage.Width; x++)
                {
                    //int index = (int)(y * stage.Width + x);
                    //pixelData[index] = map[x, y]; // <- I have to convert BGRA to RGBA pixels here
                    image[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba32(map[x, y].R, map[x, y].G, map[x, y].B, 120);
                }
            }
            _gd.Unmap(stage);

            using (var file = new System.IO.FileStream("ABC.png", System.IO.FileMode.Create))
            {
                image.Save(file, new PngEncoder());
            }

            



        }





    }
}
