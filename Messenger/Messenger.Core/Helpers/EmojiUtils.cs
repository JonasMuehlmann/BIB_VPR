using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Messenger.Core.Models;

namespace Messenger.Core.Helpers
{

    public class EmojiPicker
    {
        public List<Emoji> emojis;
        public EmojiCategorieFilter emojiCategorieFilter;

        public EmojiPicker(string emojiFilePath)
        {
            string fileContent = File.ReadAllText(emojiFilePath);
            emojis = JsonConvert.DeserializeObject<Dictionary<string, List<Emoji>>>(fileContent)["emojis"];
            emojiCategorieFilter = EmojiCategorieFilter.None;
        }

        public void AddFilter(EmojiCategorieFilter filter)
        {
            if (!emojiCategorieFilter.HasFlag(filter))
            {
                emojiCategorieFilter |= filter;
            }
        }
        public void RemoveFilter(EmojiCategorieFilter filter)
        {
            if (emojiCategorieFilter.HasFlag(filter))
            {
                emojiCategorieFilter ^= filter;
            }
        }
    }
    public class EmojiUtils
    {

    }
}
