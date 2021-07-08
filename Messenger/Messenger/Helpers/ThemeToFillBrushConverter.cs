using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Messenger.Helpers
{
    public class ThemeToFillBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (ElementTheme)value == ElementTheme.Dark ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.WhiteSmoke);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
