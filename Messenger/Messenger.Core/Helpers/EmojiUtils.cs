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
        public List<Emoji> emojisFiltered;
        public List<Emoji> emojis;
        public EmojiCategory emojiCategorieFilter;

        public EmojiPicker(string emojiFilePath)
        {
            string fileContent = File.ReadAllText(emojiFilePath);
             emojisOriginal= JsonConvert.DeserializeObject<Dictionary<string, List<Emoji>>>(fileContent)["emojis"];
            emojisFiltered = emojisOriginal;
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

        public void ResetFilters()
        {
            emojiCategorieFilter = EmojiCategory.None;
        }

        public List<Emoji> GetEmojisFromCategory(EmojiCategory category)
        {
            return emojisOriginal.Where((emoji) => {return emoji.Category == category;}).ToList();
        }

        public Dictionary<EmojiCategory, List<Emoji>> GetEmojisPerCategory()
        {
            Dictionary<EmojiCategory, List<Emoji>> emojis = new Dictionary<EmojiCategory, List<Emoji>>();

            foreach (var category in Enum.GetValues(typeof(EmojiCategory)).Cast<EmojiCategory>())
            {
                emojis[category] = GetEmojisFromCategory(category);
            }

            return emojis;
        }

        public void FilterCategories()
        {
            emojisFiltered = emojisOriginal.Where((emoji) => {
                    return emojiCategorieFilter.HasFlag(emoji.Category)
                        || emojiCategorieFilter == EmojiCategory.None;
            }).ToList();
        }
    }
    public class EmojiUtils
    {

    }
}
