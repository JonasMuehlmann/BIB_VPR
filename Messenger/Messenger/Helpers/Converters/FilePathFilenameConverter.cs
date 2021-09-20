using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    /// <summary>
    /// Returns collapsed visibility option if the value is false, else visible
    /// </summary>
    public class FilePathFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            return Path.GetFileName((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
