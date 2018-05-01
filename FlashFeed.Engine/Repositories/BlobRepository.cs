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

        public static readonly string PostsContainer = "posts";
        public static readonly string TracksContainer = "tracks";

        public static async Task<string> UploadFileAsync(string container, byte[] file, string fileName, string contentType = null)
        {
            // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
            var c = cloudBlobClient.GetContainerReference(PostsContainer);
            if (!c.Exists())
            {
                await c.CreateIfNotExistsAsync();

                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await c.SetPermissionsAsync(permissions);
            }

            switch (contentType)
            {
                case "image/jpeg":
                    fileName += ".jpeg";
                    break;
                case "image/png":
                    fileName += ".png";
                    break;
                default:
                    fileName += ".jpeg";
                    break;
            }

            CloudBlockBlob blockBlob = c.GetBlockBlobReference(fileName);
            await blockBlob.UploadFromByteArrayAsync(file, 0, file.Length);

            if (contentType != null)
            {
                blockBlob.Properties.ContentType = contentType;
                await blockBlob.SetPropertiesAsync();
            }

            return blockBlob.Uri.ToString();
        }

        public static void DeleteFile(string container, string fileIdentifier)
        {
            CloudBlobContainer c = cloudBlobClient.GetContainerReference(PostsContainer);
            CloudBlockBlob blockBlob = c.GetBlockBlobReference(fileIdentifier);

            blockBlob.DeleteIfExists();
        }
    }
}
