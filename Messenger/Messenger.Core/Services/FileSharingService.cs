using Azure.Storage.Blobs;
using Messenger.Core.Helpers;
using System.IO;
using System.Threading.Tasks;
using System;
using Serilog;
using Serilog.Context;
using Messenger.Core.Models;

namespace Messenger.Core.Services
{
    /// <summary>
    /// A class holding static methods to interact with blob storage for saving and
    /// retrieving message attachments
    /// </summary>
    public class FileSharingService
    {
        // TODO: This should not actually be hardcoded...
        private const string blobServiceConnectionString = "DefaultEndpointsProtocol=https;AccountName=bibvpr;AccountKey=5+y6T94jqFImoHa5WZLCVD+X2akF1vPJEEqZRrUeDTWlxF4TbPI7I1GMPoYX+nSzIzMXiAWxHR2Cptd/G8oJdg==;EndpointSuffix=core.windows.net";


        /// <summary>
        /// The name of the container all files are saved to
        /// </summary>
        private const string containerName = "attachments";

        /// <summary>
        /// The path at which to cache downloaded files
        /// </summary>
        public static readonly string localFileCachePath = Path.Combine(Path.GetTempPath(), "BIB_VPR" + Path.DirectorySeparatorChar);

        public static ILogger logger => GlobalLogger.Instance;

        /// <summary>
        /// Connect to our blob container
        /// </summary>
        /// <returns>BlobContainerClient object with established connection</returns>
        private static BlobContainerClient ConnectToContainer()
        {
            LogContext.PushProperty("Method", "ConnectToContainer");
            LogContext.PushProperty("SourceContext", "FileSharingService");
            logger.Information($"Function called");

            BlobServiceClient blobServiceClient = new BlobServiceClient(blobServiceConnectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }


        /// <summary>
        /// Download a file into the local cache
        /// </summary>
        /// <param name="blobFileName">A blob file to download</param>
        /// <returns>Stream on success, null otherwise</returns>
        public static async Task<MemoryStream> Download(string blobFileName)
        {
            LogContext.PushProperty("Method", "Download");
            LogContext.PushProperty("SourceContext", "FileSharingService");
            logger.Information($"Function called with parameters  blobFileName={blobFileName}");

            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                MemoryStream downloadStream = new MemoryStream();

                var result = await blobClient.DownloadToAsync(downloadStream);

                logger.Information($"Return value: {downloadStream}");

                return downloadStream;
            }
            // TODO:Find better exception(s) to catch
            catch (Exception e)
            {
                logger.Information(e, $"Return value: null");

                return null;
            }
        }


        /// <summary>
        /// Upload a file to the blob storage
        /// </summary>
        /// <param name="uploadFile">An object with file data</param>
        /// <returns>The name of the blob file on success, null otherwise</returns>
        public static async Task<string> Upload(UploadData uploadFile)
        {
            LogContext.PushProperty("Method", "Upload");
            LogContext.PushProperty("SourceContext", "FileSharingService");
            logger.Information($"Function called with parameters uploadFile={uploadFile}");

            // Adding GUID for deduplication
            string blobFileName = Path.GetFileNameWithoutExtension(uploadFile.FilePath)
                                + Path.GetExtension(uploadFile.FilePath)
                                + "." + Guid.NewGuid().ToString();

            logger.Information($"set blobFileName to {blobFileName} from uploadFile={uploadFile}");

            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                await blobClient.UploadAsync(uploadFile.StreamFile, true);

                return blobFileName;
            }
            // TODO:Find better exception(s) to catch
            catch (Exception e)
            {
                logger.Information(e, $"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Upload a base64 encoded file to the blob storage
        /// </summary>
        /// <param name="data">The base64 encoded data to upload</param>
        /// <param name="fileName">The name and extension to use for saving</param>
        /// <returns>The name of the blob file on success, null otherwise</returns>
        public static async Task<string> UploadFromBase64(string data, string fileName)
        {
            LogContext.PushProperty("Method", "UploadFromBase64");
            LogContext.PushProperty("SourceContext", "FileSharingService");
            logger.Information($"Function called with parameters data={data.Substring(0, 20)}, fileName={fileName}");

            // Adding GUID for deduplication
            string blobFileName = Path.GetFileNameWithoutExtension(fileName)
                                + Path.GetExtension(fileName)
                                + "." + Guid.NewGuid().ToString();

            logger.Information($"set blobFileName to {blobFileName} from fileName={fileName}");

            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                var bytes = Convert.FromBase64String(data);

                // Read and upload file
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    await blobClient.UploadAsync(memoryStream, true);

                    return blobFileName;
                }
            }
            // TODO:Find better exception(s) to catch
            catch (Exception e)
            {
                logger.Information(e, $"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Delete a blob file
        /// </summary>
        /// <param name="blobFileName">A blob file name to delete</param>
        /// <returns>true if successfully deleted, false otherwise</returns>
        public static async Task<bool> Delete(string blobFileName)
        {
            LogContext.PushProperty("Method", "Delete");
            LogContext.PushProperty("SourceContext", "FileSharingService");
            logger.Information($"Function called with parameters blobFileName={blobFileName}");


            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                // Read and upload file
                return await blobClient.DeleteIfExistsAsync();
            }
            // TODO:Find better exception(s) to catch
            catch (Exception e)
            {
                logger.Information(e, $"Return value: null");

                return false;
            }
        }

    }
}
