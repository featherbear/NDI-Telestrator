using NewTek.NDI;

namespace NDI_Telestrator
{
    public class BackgroundView : NewTek.NDI.WPF.ReceiveView
    {
        private Finder NDIFinder = new Finder(true);

        public System.Collections.ObjectModel.ObservableCollection<Source> Sources => NDIFinder.Sources;
        public BackgroundView()
        {
            // Add a blank input
            NDIFinder.Sources.Add(new Source());
        }

        public void setSource(Source source)
        {
            ConnectedSource = source;

            // Blank the screen if the URI is empty (User probably selected the blank input)
            if (source.Uri == null)
            {
                Disconnect();
                Child = null;
            }

            return;
        }
    }
}
