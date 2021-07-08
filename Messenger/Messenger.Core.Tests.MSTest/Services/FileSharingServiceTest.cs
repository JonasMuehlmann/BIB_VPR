using System.Threading.Tasks;
using System.IO;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class FileSharingServiceTest
    {

        [TestMethod]
        public void UploadDownload_test()
        {
            Task.Run(async () =>
            {
                string fileName = await FileSharingService.Upload(Path.GetTempFileName());

                bool success = await FileSharingService.Download(fileName);

                Assert.IsNotNull(fileName);
                Assert.IsTrue(success);

            }).GetAwaiter().GetResult();
        }
    }
}
