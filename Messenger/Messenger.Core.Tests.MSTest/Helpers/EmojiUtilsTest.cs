
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

            var emojiPicker = new EmojiPicker("/home/jonas/RiderProjects/BIB_VPR/Messenger/emojis.json");
            Console.WriteLine(emojiPicker.emojis[0]);

            Assert.IsTrue(false);
        }
    }
}
