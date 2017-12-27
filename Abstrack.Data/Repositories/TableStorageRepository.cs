using Abstrack.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Abstrack.Data.Repositories
{
    public class TableStorageRepository
    {
        public static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        public static async void AddRequestKey(RequestKey requestKey)
        {
            try
            {
                // reference requestkeys, or create if doesn't exist
                CloudTable table = tableClient.GetTableReference("requestkeys");
                await table.CreateIfNotExistsAsync();

                // add the key
                TableOperation insertOperation = TableOperation.Insert(requestKey);
                await table.ExecuteAsync(insertOperation);
            }
            catch
            {
                throw;
            }
        }
    }
}
