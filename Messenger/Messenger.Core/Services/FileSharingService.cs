using Azure.Storage.Blobs;
using Messenger.Core.Helpers;
using System.IO;
using System.Threading.Tasks;
using System;
using Serilog;
using Serilog.Context;


namespace Messenger.Core.Services
{
    public class FileSharingService
    {
        const string blobServiceConnectionString = "DefaultEndpointsProtocol=https;AccountName=vpr;AccountKey=Y/A3PMNyH7ASxIB5KobgLqeJrBGW/vNKou0Ff8MWxs3B1PbNTZ0j+Ew9PAhiMkGObziTErqZ0j693pOc+hkVHQ==;EndpointSuffix=core.windows.net";


        private const string containerName = "attachments";
        public readonly string localFileCachePath = Path.Combine(Path.GetTempPath(), "BIB_VPR" + Path.DirectorySeparatorChar);

        public ILogger logger => GlobalLogger.Instance;

        /// <summary>
        /// Connect to our blob container
        /// </summary>
        /// <returns>BlobContainerClient object with established connection</returns>
        private BlobContainerClient ConnectToContainer()
        {
            LogContext.PushProperty("Method","ConnectToContainer");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called");

            BlobServiceClient blobServiceClient = new BlobServiceClient(blobServiceConnectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        /// <summary>
        /// Download a file into the local cache
        /// </summary>
        /// <param name="blobFileName">A blob file to download</param>
        /// <param name="destinationDirectory">Where to save the file, defaults to the local application cache</param>
        /// <returns>True on success, false otherwise</returns>
        public async Task<bool> Download(string blobFileName, string destinationDirectory = "")
        {
            LogContext.PushProperty("Method","Download");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters  blobFileName={blobFileName}, destinationDirectory={destinationDirectory}");

            try
            {
                var containerClient = ConnectToContainer();

                // NOTE: destinationDirectory can not be assigned localFileCachePath
                // as a default because the expression assigning to it is not constant(compile time)
                if (destinationDirectory == string.Empty)
                {
                    destinationDirectory = localFileCachePath;

                    logger.Information($"destinationDirectory has been set to {localFileCachePath}");
                }

                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);

                    logger.Information($"created local directory {destinationDirectory}");
                }

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                var result = await blobClient.DownloadToAsync(Path.Combine(destinationDirectory, blobFileName));

                logger.Information($"Return value: {result}");

                return true;
            }
            // TODO:Find better exception(s) to catch
            catch(Exception e)
            {
                logger.Information(e, $"Return value: false");

                return false;
            }
        }

        /// <summary>
        /// Upload a file to the blob storage
        /// </summary>
        /// <param name="filePath">A path to a file to upload</param>
        /// <returns>The name of the blob file on success, null otherwise</returns>
        public async Task<string> Upload(string filePath)
        {
            LogContext.PushProperty("Method","Upload");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters filePath={filePath}");

            // Adding GUID for deduplication
            string blobFileName = Path.GetFileNameWithoutExtension(filePath)
                                + Path.GetExtension(filePath)
                                + "." + Guid.NewGuid().ToString();

            logger.Information($"set blobFileName to {blobFileName} from filePath={filePath}");

            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                // Read and upload file
                using (FileStream uploadFileStream = File.OpenRead(Path.GetFullPath(filePath)))
                {
                    await blobClient.UploadAsync(uploadFileStream, true);

                    return blobFileName;
                }
            }
            // TODO:Find better exception(s) to catch
            catch(Exception e)
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
        public async Task<bool> Delete(string blobFileName)
        {
            LogContext.PushProperty("Method","Delete");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters blobFileName={blobFileName}");


            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                    // Read and upload file
                    return await blobClient.DeleteIfExistsAsync();
            }
            // TODO:Find better exception(s) to catch
            catch(Exception e)
            {
                logger.Information(e, $"Return value: null");

                return false;
            }
        }

    }
}
