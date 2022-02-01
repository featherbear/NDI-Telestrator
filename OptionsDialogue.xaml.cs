using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

            // Fix dropdown disappearing when the main button is pressed (instead of the arrow)
            // https://github.com/featherbear/NDI-Telestrator/issues/3
            Loaded += (_sender, _evt) =>
            {
                helpers.MahApps_SplitBox_Helper.setupMainClickDropdown(NDISourcesDropdown);
                helpers.MahApps_SplitBox_Helper.setupMainClickDropdown(ScreenshotFormatDropdown);
            };
        }

        private void NDISources_Selected(object sender, SelectionChangedEventArgs e)
        {
            background.setSource((NewTek.NDI.Source)e.AddedItems[0]);
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

        #endregion

        private void onBtnLoad(object sender, RoutedEventArgs e)
        {
            InkControls.onBtnLoadClick(sender, e);
        }

        private void onBtnSave(object sender, RoutedEventArgs e)
        {

            InkControls.onBtnSaveClick(sender, e);
        }

        private void onTglQuickSave(object sender, EventArgs e)
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
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        private void onBtnAddSingle(object sender, RoutedEventArgs e)
        {
            InkControls.requestAddDrawing(false);
        }

        private void onBtnAddMultiple(object sender, RoutedEventArgs e)
        {
            InkControls.requestAddDrawing(true);
        }
    }



}
