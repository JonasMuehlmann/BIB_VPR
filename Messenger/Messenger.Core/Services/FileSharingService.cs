using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;



namespace Messenger.Core.Services
{
    public class FileSharingService : AzureServiceBase
    {
        const string blobServiceConnectionString = "DefaultEndpointsProtocol=https;AccountName=vpr;AccountKey=Y/A3PMNyH7ASxIB5KobgLqeJrBGW/vNKou0Ff8MWxs3B1PbNTZ0j+Ew9PAhiMkGObziTErqZ0j693pOc+hkVHQ==;EndpointSuffix=core.windows.net";


        private const string containerName = "attachments";
        public readonly string localFileCachePath = Path.Combine(Path.GetTempPath(), "BIB_VPR" + Path.DirectorySeparatorChar);




        /// <summary>
        /// Connect to our blob container
        /// </summary>
        /// <returns>BlobContainerClient object with established connection</returns>
        private BlobContainerClient ConnectToContainer()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobServiceConnectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        /// <summary>
        /// Download a file into the local cache
        /// </summary>
        /// <param name="blobFileName">A blob file to download</param>
        /// <returns>True on success, false otherwise</returns>
        public async Task<bool> Download(string blobFileName)
        {
            try
            {
                var containerClient = ConnectToContainer();

                if (!Directory.Exists(localFileCachePath))
                {
                    Directory.CreateDirectory(localFileCachePath);
                }

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                await blobClient.DownloadToAsync(Path.Combine(localFileCachePath, blobFileName));

                return true;
            }
            // TODO:Find better exception(s) to catch
            catch(Exception e)
            {
                Console.WriteLine(e.Message);

                return false;
            }
        }

        /// <summary>
        /// Upload a file to the blob storage
        /// </summary>
        /// <param name="blobFilePath">A path to a file to upload</param>
        /// <returns>The name of the blob file on success, null otherwise</returns>
        public async Task<string> Upload(string filePath)
        {
            // Adding GUID for deduplication
            string blobFileName = Path.GetFileNameWithoutExtension(filePath)
                                + Path.GetExtension(filePath)
                                + "." + Guid.NewGuid().ToString();

            try
            {
                var containerClient = ConnectToContainer();

                BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

                // Read and upload file
                using (FileStream uploadFileStream = File.OpenRead(Path.GetFullPath(filePath)))
                {
                    var result = await blobClient.UploadAsync(uploadFileStream, true);

                    return blobFileName;
                }
            }
            // TODO:Find better exception(s) to catch
            catch(Exception e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }
    }
}
