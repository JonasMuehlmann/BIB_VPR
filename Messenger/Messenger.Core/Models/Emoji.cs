using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace Messenger.Core.Models
{
    public class Emoji
    {
        [JsonProperty("emoji")]
        public string Emoji_    { get; set; }
        public string Name      { get; set; }
        public string ShortName { get; set; }
        public string Category  { get; set; }

        public Emoji()
        {
            Emoji_ = "";
            Name = "";
            ShortName = "";
            Category = "";
        }

        public override string ToString()
        {
            return $"Emoji: Emoji_={Emoji_}, Name={Name}, ShortName={ShortName}, Category={Category}";
        }
    }
}
