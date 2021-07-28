using Newtonsoft.Json.Linq;


namespace Messenger.Core.Models
{
    public class Emoji
    {
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
        public Emoji(JObject json)
        {
            Emoji_ = json["emoji"].ToString();
            Name = json["name"].ToString();
            ShortName = json["shortname"].ToString();
            Category = json["category"].ToString();
        }

        public override string ToString()
        {
            return $"Emoji: Emoji_={Emoji_}, Name={Name}, ShortName={ShortName}, Category={Category}";
        }
    }
}
