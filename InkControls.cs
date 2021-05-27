using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace NDI_Telestrator
{
    public static class InkControls
    {
        private static WhiteboardCanvas _whiteboard;
        public static WhiteboardCanvas whiteboard
        {
            get
            {
                return _whiteboard;
            }
            set
            {
                Console.WriteLine("SET WHITEBOARD") ;
                _whiteboard = value;
                OnPropertyChanged();
            }
        }

        private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }
        public static event PropertyChangedEventHandler PropertyChanged;


        public static void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            //saveDialog.Filter = "isf files (*.isf)|*.isf";

            //if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create);
            //    whiteboard.inkCanvas.Strokes.Save(fs);
            //    fs.Close();
            //}

        }
        public static void Btn_Load_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog();
            //openDialog.Filter = "isf files (*.isf)|*.isf";

            //if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    FileStream fs = new FileStream(openDialog.FileName, FileMode.Open);
            //    whiteboard.inkCanvas.Strokes = new System.Windows.Ink.StrokeCollection(fs);
            //    fs.Close();
            //    whiteboard.updateUndoRedoStates();
            //}
        }

        public static void onBtnWhiteClick(object sender, RoutedEventArgs e)
        {
            whiteboard.Background = Brushes.White;
        }
        public static void onBtnChromaClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            whiteboard.Background = btn.Foreground;
        }
        public static void onBtnTransparentClick(object sender, RoutedEventArgs e)
        {
            whiteboard.Background = Brushes.Transparent;
        }

        public static void onBtnPenClick(object sender, RoutedEventArgs e)
        {
            /*
            foreach (UIElement control in DrawSettings.Children)
            {
                if (control is Border)
                {
                    Border border = control as Border;
                    border.BorderBrush = Brushes.Transparent;
                }
            }
            */

            Button btn = (Button)sender;
            //btn.BorderBrush = Brushes.Red;

            whiteboard.SetPenColor(btn.Foreground);
        }

        public static void clearWhiteboard()
        {
            whiteboard.Clear();
        }
        public static void setPenThickness(double size)
        {
            whiteboard.SetPenThickness(size);
        }

        public static void undo()
        {
            whiteboard.Undo();

            // Previews
            //ABC.Children.Clear();

            //for (int i = 1; i < theWhiteboard.inkCanvas.Strokes.Count; i++)
            //{
            //    List<System.Windows.Ink.Stroke> a = new List<System.Windows.Ink.Stroke>(theWhiteboard.inkCanvas.Strokes);
            //    int offset = 0;
            //    int amt = i;

            //    a = a.GetRange(offset, System.Math.Max(0, System.Math.Min(amt, a.Count - offset)));

            //    Image img = new Image();
            //    img.Width = 640;
            //    img.Height = 360;
            //    img.Source = Draw(new System.Windows.Ink.StrokeCollection(a));

            //    ABC.Children.Add(img);
            //}
        }

        public static void redo()
        {
            whiteboard.Redo();
        }

        // TODO: Move somewhere else
        public static BitmapFrame Draw(System.Windows.Ink.StrokeCollection[] layers, Brush background = null)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                if (background != null)
                {
                    drawingContext.DrawRectangle(background, null, new Rect(0, 0, (int)whiteboard.activeInkCanvas.Width, (int)whiteboard.activeInkCanvas.Height));

                }

                foreach (System.Windows.Ink.StrokeCollection strokes in layers)
                {
                    strokes.Draw(drawingContext);
                };
                drawingContext.Close();

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)whiteboard.activeInkCanvas.Width, (int)whiteboard.activeInkCanvas.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                rtb.Render(drawingVisual);

                BitmapFrame B = BitmapFrame.Create(rtb);
                return B;
            }
        }

        public static void Btn_Screenshot_Click(object sender, RoutedEventArgs e)
        {
            Enums.ScreenshotFormatTypes type = Options.screenshotFormatType;

            String saveFileName = "Screenshot " + DateTime.Now.ToString("yyyyMMdd-HHmmss");

            if (!Options.quickSaveEnabled)
            {
                System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
                saveDialog.Filter = "JPG|*.jpg|PNG|*.png";
                saveDialog.FilterIndex = (int)Options.screenshotFormatType + 1; // Indexing is 1-based

                System.Windows.Forms.DialogResult result = saveDialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK) return;

                saveFileName = saveDialog.FileName;
                type = (Enums.ScreenshotFormatTypes)(saveDialog.FilterIndex - 1);

                string lowerCase = saveDialog.FileName.ToLower();
                if (lowerCase.EndsWith(".jpg")) type = Enums.ScreenshotFormatTypes.JPG;
                else if (lowerCase.EndsWith(".png")) type = Enums.ScreenshotFormatTypes.PNG;
                else saveFileName += (saveDialog.FilterIndex == 1 ? ".jpg" : ".png");
            }
            else
            {
                saveFileName += type == Enums.ScreenshotFormatTypes.JPG ? ".jpg" : ".png";
            }

            BitmapFrame b;
            if (type == Enums.ScreenshotFormatTypes.JPG)
            {


                b = Draw(whiteboard.InkCanvases.Select(c => c.Strokes).ToArray(), whiteboard.Background == Brushes.Transparent ? Brushes.White : whiteboard.Background);
                JpegBitmapEncoder j = new JpegBitmapEncoder();
                j.Frames.Add(b);

                using (var file = new FileStream(saveFileName, FileMode.Create))
                {
                    j.Save(file);
                }

            }
            else
            {
                b = Draw(whiteboard.InkCanvases.Select(c => c.Strokes).ToArray(), Brushes.Transparent);
                PngBitmapEncoder p = new PngBitmapEncoder();
                p.Frames.Add(b);

                using (var file = new FileStream(saveFileName, FileMode.Create))
                {
                    p.Save(file);
                }
            }
        }

        public static void createNewLayer()
        {
            whiteboard.addNewLayer();
        }

        public static void setActiveLayer(int index)
        {
            if (index >= whiteboard.Children.Count)
            {
                Console.WriteLine("Got index " + index + " but child count is " + whiteboard.Children.Count);
                return;
                throw new Exception("OOB");
            }
            whiteboard.activeInkCanvas = (InkCanvas)whiteboard.Children[index];
            Console.WriteLine("Set active");
        }
    }
}
