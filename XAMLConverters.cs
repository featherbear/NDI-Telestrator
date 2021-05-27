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
    }
}
