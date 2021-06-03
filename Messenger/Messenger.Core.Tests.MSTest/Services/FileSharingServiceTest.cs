using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;
using System.Data.SqlClient;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class FileSharingServiceTest : SqlServiceTestBase
    {
        FileSharingService fileSharingService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            fileSharingService = InitializeTestMode<FileSharingService>();
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
