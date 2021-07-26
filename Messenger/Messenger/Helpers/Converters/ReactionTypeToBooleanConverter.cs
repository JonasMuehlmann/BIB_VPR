using Messenger.Models;
using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    /// <summary>
    /// Returns true if the reaction type matches my reaction, else false
    /// </summary>
    public class ReactionTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null
                || !(value is ReactionType))
            {
                return false;
            }

            ReactionType type = (ReactionType)value;
            ReactionType param = (ReactionType)Enum.Parse(typeof(ReactionType), parameter.ToString());

            return type == param ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return false;
        }
    }
}
