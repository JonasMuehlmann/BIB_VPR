using System.Text.Json;
using System.Collections.Generic;

namespace Messenger.Core.Helpers
{
    /// <summary>
    /// Holds static methods to work with Notification Messages
    /// </summary>
    public static class NotificationMessage
    {
        public static string UserMentionedToJson(Dictionary<object, object> properties)
        {
            return JsonSerializer.Serialize(properties);
        }

        public static Dictionary<string, string> ToDict(string properties)
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(properties);
        }
    }
}
