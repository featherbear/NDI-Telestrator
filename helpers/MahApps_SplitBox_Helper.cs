using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace NDI_Telestrator.helpers
{
    class MahApps_SplitBox_Helper
    {
        public static void setupMainClickDropdown(MahApps.Metro.Controls.SplitButton obj)
        {
            var objButton = (Button)obj.GetType().GetField("button", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
            var evtButtonClick = (RoutedEventHandler)obj.GetType()
                .GetMethod("ButtonClick", BindingFlags.Instance | BindingFlags.NonPublic)
                .CreateDelegate(typeof(RoutedEventHandler), obj);

            objButton.Click -= evtButtonClick;
            objButton.Click += (sender, evt) =>
            {
                obj.IsDropDownOpen = !obj.IsDropDownOpen;
            };
        }
    }
}
