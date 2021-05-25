using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDI_Telestrator.Enums;
using System.Windows.Markup;
using System.ComponentModel;

namespace NDI_Telestrator
{
    namespace Enums
    {
        #region Helpers

        /// <summary>
        /// https://stackoverflow.com/a/17405771
        /// </summary>
        public class EnumToItemsSource : MarkupExtension
        {
            private readonly Type _type;

            public EnumToItemsSource(Type type)
            {
                _type = type;
            }

            public override object ProvideValue(IServiceProvider serviceProvider)
            {
                return Enum.GetValues(_type)
                    .Cast<object>()
                    .Select(e => new EnumDefinition(e));
            }
        }

        class EnumDefinition
        {
            public EnumDefinition(Object obj)
            {
                Value = (int)obj;
                DisplayName = obj.ToString();
            }
            public int Value { get; set; }
            public string DisplayName { get; set; }
        }

        #endregion

        public enum ScreenshotFormatTypes
        {
            JPG,
            PNG
        }
    }

    public static class Options // : INotifyPropertyChanged
    {
        #region Helpers
        static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }
        public static event PropertyChangedEventHandler PropertyChanged;
        #endregion


        private static ScreenshotFormatTypes _screenshotFormatType = ScreenshotFormatTypes.PNG;
        public static ScreenshotFormatTypes screenshotFormatType
        {
            get
            {
                return _screenshotFormatType;
            }
            set
            {
                _screenshotFormatType = value;
                OnPropertyChanged();
            }
        }


        private static bool _quickSaveEnabled = true;
        public static bool quickSaveEnabled
        {
            get
            {
                return _quickSaveEnabled;
            }
            set
            {
                _quickSaveEnabled = value;
                OnPropertyChanged();
            }
        }

    }
}


