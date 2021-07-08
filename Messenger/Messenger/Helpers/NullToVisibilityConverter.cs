using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    /// <summary>
    /// Returns collapsed visibility option if the value is null, else visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isNull = value == null;
            bool emptyString = false;
            bool emptyList = false;

            if (!isNull)
            {
                emptyString = value is string
                    && string.IsNullOrEmpty((string)value);

                emptyList = value is IList
                    && (value as IList).Count == 0;
            }

            bool isCollapsed = isNull || emptyString || emptyList;

            if (parameter != null && parameter.ToString() == "invert")
            {
                return isCollapsed ? Visibility.Visible : Visibility.Collapsed;
            }

            return isCollapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
