using Abstrack.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Abstrack.Functions.Repositories
{
    public class TableStorageRepository
    {
        public static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        public static async Task<RequestKey> ValidateRequestKey(string requestKey)
        {
            // get the entity
            CloudTable table = tableClient.GetTableReference("requestkeys");

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<RequestKey>("track_request_key", requestKey);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (RequestKey)retrievedResult.Result;
        }
    }
}
