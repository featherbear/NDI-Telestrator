using MahApps.Metro.Controls;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Forms = System.Windows.Forms;

namespace NDI_Telestrator
{
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Z:
                    if ((Forms.Control.ModifierKeys & Forms.Keys.Control) == Forms.Keys.Control) theWhiteboard.Undo();
                    break;
            }
        }

        private void Btn_White_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.Background = Brushes.White;
        }
        private void Btn_Chroma_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            theWhiteboard.Background = btn.Foreground;
        }
        private void Btn_Transparent_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.Background = Brushes.Transparent;
        }

        private void Btn_Pen_Click(object sender, RoutedEventArgs e)
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

            theWhiteboard.SetPenColor(btn.Foreground);
        }
        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.Filter = "isf files (*.isf)|*.isf";

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(saveFileDialog1.FileName,
                                               FileMode.Create);
                theWhiteboard.inkCanvas.Strokes.Save(fs);
                fs.Close();
            }

        }
        private void Btn_Load_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog openFileDialog1 = new Forms.OpenFileDialog();
            openFileDialog1.Filter = "isf files (*.isf)|*.isf";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(openFileDialog1.FileName,
                                               FileMode.Open);
                theWhiteboard.inkCanvas.Strokes = new System.Windows.Ink.StrokeCollection(fs);
                fs.Close();
            }
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.Clear();
        }
        private void Btn_Size1_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.SetPenThickness(1.0);
        }

        private void Btn_Size2_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.SetPenThickness(2.0);
        }

        private void Btn_Size3_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.SetPenThickness(3.0);
        }

        private void Btn_Size4_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.SetPenThickness(4.0);
        }

        private void Btn_Size5_Click(object sender, RoutedEventArgs e)
        {
            theWhiteboard.SetPenThickness(5.0);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            theBackground.setSource((NewTek.NDI.Source) ((ComboBox)sender).SelectedItem);
        }
    }
}