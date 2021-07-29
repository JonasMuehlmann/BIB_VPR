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

        public EmojiPicker(string emojiFilePath)
        {
            string fileContent = File.ReadAllText(emojiFilePath);
            emojis = JsonConvert.DeserializeObject<Dictionary<string, List<Emoji>>>(fileContent)["emojis"];
        }
    }
    public class EmojiUtils
    {

    }
}
