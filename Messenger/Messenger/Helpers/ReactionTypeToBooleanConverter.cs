using Messenger.Models;
using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    public class ReactionTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null
                || !(value is string))
            {
                return false;
            }

            ReactionType type = (ReactionType)value;
            ReactionType param = (ReactionType)Enum.Parse(typeof(ReactionType), parameter.ToString());

            return type == param ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
