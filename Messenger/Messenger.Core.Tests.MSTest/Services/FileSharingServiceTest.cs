using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using Messenger.Core.Services;
using Messenger.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class FileSharingServiceTest
    {
        [TestMethod]
        public void UploadDownload_test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var unicodeEncoding = new UnicodeEncoding();
                var streamContent   = unicodeEncoding.GetBytes(testName + "Content");
                var memStream       = new MemoryStream(streamContent.Length);

                memStream.Write(streamContent, 0, streamContent.Length);
                memStream.Seek(0, SeekOrigin.Begin);

                var blobFilename = await FileSharingService.Upload(new UploadData(memStream, $"{Path.GetTempPath()}{testName}File.txt"));
                Assert.IsNotNull(blobFilename);

                var resultStream = await FileSharingService.Download(blobFilename);
                Assert.IsNotNull(resultStream);

                var resultContent = resultStream.ToArray();
                resultStream.Read(resultContent , 0, streamContent.Length);
                Assert.AreEqual(BitConverter.ToString(streamContent), BitConverter.ToString(resultContent));

                var didDelete = await FileSharingService.Delete(blobFilename);
                Assert.IsTrue(didDelete);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }
    }
}
