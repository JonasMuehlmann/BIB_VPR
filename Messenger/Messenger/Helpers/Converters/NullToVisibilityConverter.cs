using System;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    /// <summary>
    /// Returns collapsed visibility option if the value is null, else visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && int.TryParse(value.ToString(), out int count))
            {
                if (parameter != null && parameter.ToString() == "invert")
                {
                    return count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return count == 0 ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            bool isNull = value == null;
            bool emptyString = false;
            bool emptyList = false;

            if (!isNull)
            {
                emptyString = string.IsNullOrEmpty(value.ToString());

                emptyList = value is IList
                    && (value as IList).Count <= 0;
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
