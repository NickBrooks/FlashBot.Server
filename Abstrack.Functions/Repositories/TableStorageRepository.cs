﻿using Abstrack.Entities;
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
        private static readonly string TrackTagsTable = "tracktags";
        private static readonly string RequestKeysTable = "requestkeys";

        public static async Task<RequestKey> GetRequestKey(string requestKey)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(RequestKeysTable);

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<RequestKey>("track_request_key", requestKey);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (RequestKey)retrievedResult.Result;
        }

        public static async void UpdateRequestKey(RequestKey requestKey)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(RequestKeysTable);

            // update
            TableOperation updateOperation = TableOperation.Replace(requestKey);
            await table.ExecuteAsync(updateOperation);
        }

        public static async void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(TrackTagsTable);

            var exists = await GetTrackTag(trackTag.PartitionKey, trackTag.RowKey);

            if (exists != null)
            {
                // update
                exists.count++;
                TableOperation replace = TableOperation.Replace(exists);
                await table.ExecuteAsync(replace);
            }
            else
            {
                // insert
                trackTag.count = 1;
                TableOperation insertOrReplace = TableOperation.InsertOrReplace(trackTag);
                await table.ExecuteAsync(insertOrReplace);
            }

        }

        public static async Task<TrackTag> GetTrackTag(string trackId, string tag)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(TrackTagsTable);

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<TrackTag>(trackId, tag);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (TrackTag)retrievedResult.Result;
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
