using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Messenger.Core.Models
{
    public class Emoji
    {
        [JsonProperty("emoji")]
        public string Emoji_           { get; set; }
        public string Name             { get; set; }
        public string ShortName        { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("category")]
        public EmojiCategory Category  { get; set; }

        public Emoji()
        {
            Emoji_ = "";
            Name = "";
            ShortName = "";
        }

        public override string ToString()
        {
            return $"Emoji: Emoji_={Emoji_}, Name={Name}, ShortName={ShortName}, Category={Category}";
        }
    }
}
