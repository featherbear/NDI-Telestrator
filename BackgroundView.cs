using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using NewTek.NDI;


namespace NDI_Telestrator
{
    class BackgroundView : NewTek.NDI.WPF.ReceiveView
    {
        private NewTek.NDI.Finder NDIFinder = new NewTek.NDI.Finder(true);

        public System.Collections.ObjectModel.ObservableCollection<Source> Sources => NDIFinder.Sources;
        public BackgroundView()
        {
            NDIFinder.Sources.Add(new Source());

            //NDIFinder.Sources.CollectionChanged += (obj, newSources) =>

            //{
            //    System.Console.WriteLine(newSources.NewItems[0]);
            //    this.Dispatcher.Invoke(() =>
            //    {
            //        this.ConnectedSource = ((NewTek.NDI.Source)newSources.NewItems[0]);
            //    });
            //};
        }

        public void setSource(Source source)
        {
            this.ConnectedSource = source;
            return;
        }

        internal void setSource(SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
