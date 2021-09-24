using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    /// <summary>
    /// Sets horizontal alignment to right if the message is from the currently logged in user, else to the left
    /// </summary>
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
