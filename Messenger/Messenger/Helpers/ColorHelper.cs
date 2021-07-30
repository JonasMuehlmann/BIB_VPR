using System.Globalization;
using Windows.UI;

namespace Messenger.Helpers
{
    public static class ColorHelper
    {
        public static Color ToColor(this string value)
        {
            byte r = 255;
            byte g = 255;
            byte b = 255;

            if (value == null) return Color.FromArgb(255, r, g, b);

            if (byte.TryParse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte rValue))
            {
                r = rValue;
            }

            if (byte.TryParse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte gValue))
            {
                g = gValue;
            }

            if (byte.TryParse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte bValue))
            {
                b = bValue;
            }

            return Color.FromArgb(255, r, g, b);
        }

        public static string ToHex(this Color color)
        {
            return string.Concat(color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
        }
    }
}
