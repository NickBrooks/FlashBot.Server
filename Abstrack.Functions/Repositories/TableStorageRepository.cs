using Abstrack.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Abstrack.Functions.Repositories
{
    public class TableStorageRepository
    {
        public static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        public static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        public static async Task<RequestKey> GetRequestKey(string requestKey)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference("requestkeys");

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<RequestKey>("track_request_key", requestKey);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (RequestKey)retrievedResult.Result;
        }

        public static async void UpdateRequestKey(RequestKey requestKey)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference("requestkeys");

            // update
            TableOperation updateOperation = TableOperation.Replace(requestKey);
            await table.ExecuteAsync(updateOperation);
        }

        public static async void InsertOrReplaceTrackTag(TrackTag trackTag)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference("tracktags");

            // update
            TableOperation insertOrReplace = TableOperation.InsertOrReplace(trackTag);
            await table.ExecuteAsync(insertOrReplace);
        }

        public static void AddMessageToQueue(string queueName, string messageBody)
        {
            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(messageBody);
            queue.AddMessage(message);
        }
    }
}
