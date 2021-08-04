using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    /// <summary>
    /// Returns collapsed visibility option if the value is false, else visible
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter != null && parameter.ToString() == "invert")
            {
                return (int)value == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return (int)value == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
