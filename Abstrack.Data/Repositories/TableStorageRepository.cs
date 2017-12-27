using Abstrack.Data.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Abstrack.Data.Repositories
{
    class TableStorageRepository
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

        public static async Task<string> GetTrackId(string requestKey)
        {
            // get the entity
            CloudTable table = tableClient.GetTableReference("requestkeys");

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<RequestKey>("track_request_key", requestKey);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            var result = (RequestKey)retrievedResult.Result;

            return result.track_id;
        }
    }
}
