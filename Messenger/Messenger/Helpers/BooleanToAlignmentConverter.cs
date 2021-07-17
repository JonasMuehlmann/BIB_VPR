using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    public class BooleanToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is bool))
            {
                return null;
            }

            bool isMyMessage = (bool)value;

            return isMyMessage ? "Right" : "Left";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
