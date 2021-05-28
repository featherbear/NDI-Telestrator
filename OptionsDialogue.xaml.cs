using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NDI_Telestrator
{
    /// <summary>
    /// Interaction logic for OptionsDialogue.xaml
    /// </summary>
    public partial class OptionsDialogue : MahApps.Metro.Controls.Flyout, INotifyPropertyChanged
    {

        private BackgroundView _background;
        public BackgroundView background
        {
            get
            {
                return _background;
            }
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }






        private WhiteboardCanvas _whiteboard;
        public WhiteboardCanvas whiteboard
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





        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public OptionsDialogue()
        {
            InitializeComponent();

        }

        private void NDISources_Selected(object sender, SelectionChangedEventArgs e)
        {
            background.setSource((NewTek.NDI.Source)e.AddedItems[0]);
        }

        public ICommand handleOpenNDISourceDropdown
        {
            get
            {
                return new SimpleCommand(o => NDISourcesDropdown.IsExpanded = true);
            }
        }


        #region Screenshot Options


        public int bindScreenshotFormatTypeIndex
        {
            get
            {
                return (int)Options.screenshotFormatType;
            }

            set
            {
                Options.screenshotFormatType = (Enums.ScreenshotFormatTypes)value;
            }
        }


        public ICommand handleOpenScreenshotFormatDropdown
        {
            get
            {
                return new SimpleCommand(o => ScreenshotFormatDropdown.IsExpanded = true);
            }
        }

        #endregion

        private void onBtnLoad(object sender, RoutedEventArgs e)
        {
            InkControls.onBtnLoadClick(sender, e);
        }

        private void onBtnSave(object sender, RoutedEventArgs e)
        {

            InkControls.onBtnSaveClick(sender, e);
        }

        private void ToggleSwitch_IsCheckedChanged(object sender, EventArgs e)
        {
            Options.quickSaveEnabled = !Options.quickSaveEnabled;
        }

        private void onBtnCreateLayer(object sender, RoutedEventArgs e)
        {
            InkControls.createNewLayer();
            Console.WriteLine("There are now " + InkControls.whiteboard.Children.Count + " canvases");
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Persist visible selection 
            ListBox obj = (ListBox)sender;

            if (obj.SelectedIndex == -1) return;
            selectedIndex = obj.SelectedIndex;

            InkControls.setActiveLayer(obj.Items.Count - selectedIndex - 1);
        }

        private int _selectedIndex = 0;
        public int selectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                Console.WriteLine("UPDATE");
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        private void onBtnAddSingle(object sender, RoutedEventArgs e)
        {

        }

        private void onBtnAddMultiple(object sender, RoutedEventArgs e)
        {

        }
    }



}
