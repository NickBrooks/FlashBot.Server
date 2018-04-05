using Abstrack.Engine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abstrack.Functions.Repositories
{
    public class TableStorageRepository
    {
        public static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        public static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static readonly string TrackTagsTable = "tracktags";
        private static readonly string TracksTable = "tracks";

        public static async Task<Track> GetTrackByRequestKey(string requestKey)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);

                // query tracks
                TableQuery<Track> rangeQuery = new TableQuery<Track>().Where(TableQuery.GenerateFilterCondition("Request_Key", QueryComparisons.Equal, requestKey));

                TableContinuationToken token = null;

                TableQuerySegment<Track> tableQueryResult =
                    await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                return tableQueryResult.FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public static async void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(TrackTagsTable);

            var exists = await GetTrackTag(trackTag.PartitionKey, trackTag.RowKey);

            if (exists != null)
            {
                // update
                exists.Count++;
                TableOperation replace = TableOperation.Replace(exists);
                await table.ExecuteAsync(replace);
            }
            else
            {
                // insert
                trackTag.Count = 1;
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
