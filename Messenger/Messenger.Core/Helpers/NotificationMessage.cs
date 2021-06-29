using System.Text.Json;
using System.Collections.Generic;

namespace Messenger.Core.Helpers
{
    /// <summary>
    /// Handles serialization and deserialization of notification messages
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
