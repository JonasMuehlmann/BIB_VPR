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
        public EmojiCategory emojiCategorieFilter;

        public EmojiPicker(string emojiFilePath)
        {
            string fileContent = File.ReadAllText(emojiFilePath);
            emojis = JsonConvert.DeserializeObject<Dictionary<string, List<Emoji>>>(fileContent)["emojis"];
            emojiCategorieFilter = EmojiCategory.None;
        }

        public void AddFilter(EmojiCategory filter)
        {
            if (!emojiCategorieFilter.HasFlag(filter))
            {
                emojiCategorieFilter |= filter;
            }
        }
        public void RemoveFilter(EmojiCategory filter)
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
