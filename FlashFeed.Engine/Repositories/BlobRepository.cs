using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class BlobRepository
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));

        private static CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

        private static readonly string PostsContainer = "posts";

        public static async Task<string> UploadFileAsync(byte[] file, string fileName, string contentType)
        {
            // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(PostsContainer);
            if (!cloudBlobContainer.Exists())
            {
                await cloudBlobContainer.CreateIfNotExistsAsync();

                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);
            }

            switch (contentType)
            {
                case "image/jpeg":
                    fileName += ".jpeg";
                    break;
                case "image/png":
                    fileName += ".png";
                    break;
                case "image/gif":
                    fileName += ".gif";
                    break;
                default:
                    fileName += ".jpeg";
                    break;
            }

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
            await blockBlob.UploadFromByteArrayAsync(file, 0, file.Length);

            blockBlob.Properties.ContentType = contentType;
            await blockBlob.SetPropertiesAsync();

            return blockBlob.Uri.ToString();
        }
    }
}
