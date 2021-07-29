
using System;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
            using System.Diagnostics;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class EmojiUtilsTest
    {
        [TestMethod]
        public void test_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            var emojiPicker = new EmojiPicker("/home/jonas/RiderProjects/BIB_VPR/Messenger/emojis.json");

            emojiPicker.AddFilter(EmojiCategory.Smileys);
            emojiPicker.AddFilter(EmojiCategory.FoodDrink);
            emojiPicker.AddFilter(EmojiCategory.Component);

            emojiPicker.FilterCategories();

            sw.Stop();

            Console.WriteLine("Elapsed Ms={0}",sw.Elapsed.TotalMilliseconds);

            Assert.IsTrue(false);
        }
    }
}
