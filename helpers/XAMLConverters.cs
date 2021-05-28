using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NDI_Telestrator
{
    namespace XAMLConverters
    {
        public class ReverseConverter : IValueConverter
        // https://stackoverflow.com/a/7506217
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return ((IEnumerable<object>)value).Reverse();
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class ColorToSolidColorBrushNoTransparent : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                System.Windows.Media.Color colour = (System.Windows.Media.Color)value;
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(colour.R, colour.G, colour.B));
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
