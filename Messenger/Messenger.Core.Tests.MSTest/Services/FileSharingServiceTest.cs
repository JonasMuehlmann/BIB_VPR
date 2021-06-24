using System.Threading.Tasks;
using System.IO;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class FileSharingServiceTest
    {
        FileSharingService fileSharingService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            fileSharingService = new FileSharingService();
        }

        [TestMethod]
        public void UploadDownload_test()
        {
            Task.Run(async () =>
            {
                string fileName = await fileSharingService.Upload(Path.GetTempFileName());

                bool success = await fileSharingService.Download(fileName);

                Assert.IsNotNull(fileName);
                Assert.IsTrue(success);

            }).GetAwaiter().GetResult();
        }
    }
}
