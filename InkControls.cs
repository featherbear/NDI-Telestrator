using System;
using System.Collections.Generic;
using System.IO;
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
                _whiteboard = value;
                OnPropertyChanged();
            }
        }

        private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }
        public static event PropertyChangedEventHandler PropertyChanged;


        public static void onBtnSaveClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            saveDialog.Filter = "Telestrator File (*.tls)|*.tls";

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create);

                foreach (InkLayer layer in whiteboard.Children) layer.Strokes.Save(fs);

                fs.Close();
            }

        }

        private static List<System.Windows.Ink.StrokeCollection> requestOpenFile()
        {
            List<System.Windows.Ink.StrokeCollection> layers = new List<System.Windows.Ink.StrokeCollection>();

            System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog();
            openDialog.Filter = "Ink Stroke File (*.isf)|*.isf|Telestrator File (*.tls)|*.tls|All supported files|*.isf;*.tls";
            openDialog.FilterIndex = 3;

            if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return null;

            FileStream fs = new FileStream(openDialog.FileName, FileMode.Open);
            while (fs.Position != fs.Length) layers.Add(new System.Windows.Ink.StrokeCollection(fs));

            fs.Close();

            return layers;
        }

        public static void requestAddDrawing(bool asMultipleLayers)
        {
            List<System.Windows.Ink.StrokeCollection> layers = requestOpenFile();
            if (layers == null || layers.Count == 0) return;

            if (asMultipleLayers)
            {
                foreach (System.Windows.Ink.StrokeCollection layer in layers) whiteboard.addNewLayer(layer);
            }
            else
            {
                System.Windows.Ink.StrokeCollection sum = new System.Windows.Ink.StrokeCollection();
                foreach (System.Windows.Ink.StrokeCollection layer in layers) sum.Add(layer);
                whiteboard.addNewLayer(sum);
            }
        }

        public static void onBtnLoadClick(object sender, RoutedEventArgs e)
        {
            List<System.Windows.Ink.StrokeCollection> layers = requestOpenFile();
            if (layers == null || layers.Count == 0) return;

            whiteboard.ResetState(false);
            foreach (System.Windows.Ink.StrokeCollection layer in layers) whiteboard.addNewLayer(layer);
        }

      
        public static void setPenColour(Color colour)
        {
            whiteboard.SetPenColour(colour);
        }

        public static void setBackgroundColour(Brush colour)
        {
            whiteboard.Background = colour;
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

        // Draw a selective number of layers
        public static BitmapFrame Draw(System.Windows.Ink.StrokeCollection[] layers, Brush background = null)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                if (background != null) drawingContext.DrawRectangle(background, null, new Rect(0, 0, (int)whiteboard.activeInkCanvas.Width, (int)whiteboard.activeInkCanvas.Height));
                foreach (System.Windows.Ink.StrokeCollection strokes in layers) strokes.Draw(drawingContext);
                drawingContext.Close();

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)whiteboard.activeInkCanvas.Width, (int)whiteboard.activeInkCanvas.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                rtb.Render(drawingVisual);

                return BitmapFrame.Create(rtb);
            }
        }

        public static void Btn_Screenshot_Click(object sender, RoutedEventArgs e)
        {
            Enums.ScreenshotFormatTypes type = Options.screenshotFormatType;

            string saveFileName = "Screenshot " + DateTime.Now.ToString("yyyyMMdd-HHmmss");

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
            else saveFileName += type == Enums.ScreenshotFormatTypes.JPG ? ".jpg" : ".png";


            BitmapFrame b;
            if (type == Enums.ScreenshotFormatTypes.JPG)
            {
                b = whiteboard.Draw(whiteboard.Background == Brushes.Transparent ? Brushes.White : whiteboard.Background);
                JpegBitmapEncoder j = new JpegBitmapEncoder();
                j.Frames.Add(b);

                using (var file = new FileStream(saveFileName, FileMode.Create)) j.Save(file);

            }
            else
            {
                b = whiteboard.Draw(Brushes.Transparent);
                PngBitmapEncoder p = new PngBitmapEncoder();
                p.Frames.Add(b);

                using (var file = new FileStream(saveFileName, FileMode.Create)) p.Save(file);
            }
        }

        public static void createNewLayer()
        {
            whiteboard.addNewLayer();
        }

        public static void setActiveLayer(int index)
        {
            whiteboard.setActive(index);
        }

    }
}
