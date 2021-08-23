using System;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    public class ColorToHexCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color)
            {
                Color color = (Color)value;
                return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
