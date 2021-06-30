using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    public class BooleanToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "\uE971" : "\uE972";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
