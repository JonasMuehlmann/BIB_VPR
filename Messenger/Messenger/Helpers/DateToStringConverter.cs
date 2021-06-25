using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime)
            {
                DateTime date = DateTime.Parse(value.ToString());

                string result = string.Empty;
                TimeSpan elapsed = DateTime.Now - date;
                if (elapsed < TimeSpan.FromDays(1))
                {
                    result += "Today, ";
                }
                else if (elapsed < TimeSpan.FromDays(2))
                {
                    result += "Yesterday, ";
                }
                else
                {
                    TimeSpan span = DateTime.Now - date;
                    result += $"{span.Days} days ago ";
                }

                result += $"@ {date.ToString("HH:mm")}";

                return result;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return string.Empty;
        }
    }
}
