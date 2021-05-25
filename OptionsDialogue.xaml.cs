﻿using System;
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

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;




        // public InkControls controls { get; set; }
        //        public BackgroundView background { get; set; }

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

        public static List<String> ScreenshotFormatEnum { get { return new List<String> { "JPG", "PNG" }; } }


        private void ScreenshotFormat_Selected(object sender, SelectionChangedEventArgs e)
        {
            System.Console.WriteLine(e);

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
            InkControls.Btn_Load_Click(sender, e);
        }

        private void onBtnSave(object sender, RoutedEventArgs e)
        {

            InkControls.Btn_Save_Click(sender, e);
        }


    }
}
