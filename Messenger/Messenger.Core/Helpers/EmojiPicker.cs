using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Messenger.Core.Models;
using DuoVia.FuzzyStrings;

namespace Messenger.Core.Helpers
{
    public class EmojiPicker
    {
        private List<Emoji> emojisOriginal;
        public List<Emoji> emojis;
        public EmojiCategory emojiCategorieFilter;

        public EmojiPicker()
        {
            string fileContent = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets/emojis.json"));
            emojisOriginal = JsonConvert.DeserializeObject<Dictionary<string, List<Emoji>>>(fileContent)["emojis"];
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
            emojis = emojisOriginal.Where((emoji) => emojiCategorieFilter.HasFlag(emoji.Category)).ToList();
        }

        public void Rank(string searchTerm)
        {
            emojis = emojis.OrderByDescending(emoji => LongestCommonSubsequenceExtensions.LongestCommonSubsequence(emoji.Name, searchTerm).Item2 * 100.0).ToList();
        }
    }
}
