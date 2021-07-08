using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    /// <summary>
    /// Returns caret symbol if the value is true, else down caret symbol
    /// </summary>
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
