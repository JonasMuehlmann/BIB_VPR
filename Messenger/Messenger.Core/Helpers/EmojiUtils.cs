using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Messenger.Core.Models;

namespace Messenger.Core.Helpers
{

    public class EmojiPicker
    {
        private List<Emoji> emojisOriginal;
        public List<Emoji> emojis;
        public EmojiCategory emojiCategorieFilter;

        public EmojiPicker(string emojiFilePath)
        {
            string fileContent = File.ReadAllText(emojiFilePath);
             emojisOriginal= JsonConvert.DeserializeObject<Dictionary<string, List<Emoji>>>(fileContent)["emojis"];
            emojis = emojisOriginal;
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

        public void FilterCategories()
        {
            emojis = emojisOriginal.Where((emoji) => {return emojiCategorieFilter.HasFlag(emoji.Category) || emojiCategorieFilter == EmojiCategory.None;}).ToList();
        }
    }
    public class EmojiUtils
    {

    }
}
