using Abstrack.Engine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abstrack.Engine.Repositories
{
    public class TableStorageRepository
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));
        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static readonly string TrackTagsTable = "tracktags";
        private static readonly string TracksTable = "tracks";
        private static readonly string ExtendedUsersTable = "extendedusers";

        /// <summary>
        /// Insert track into Table Storage.
        /// </summary>
        /// <param name="track"></param>
        /// <returns>Returns the inserted track.</returns>
        internal static async Task<Track> CreateTrack(Track track)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);
                await table.CreateIfNotExistsAsync();

                // insert the track
                TableOperation op = TableOperation.Insert(track);
                var result = await table.ExecuteAsync(op);
                if (result == null)
                    return null;

                return track;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get track from Table Storage.
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns>Returns the inserted track.</returns>
        internal static async Task<Track> GetTrack(string trackId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);

                // query tracks
                TableQuery<Track> rangeQuery = new TableQuery<Track>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, trackId));

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

        /// <summary>
        /// Delete the track.
        /// </summary>
        /// <param name="trackId"></param>
        internal static async void DeleteTrack(string trackId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);

                // query tracks
                TableQuery<Track> rangeQuery = new TableQuery<Track>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, trackId));

                TableContinuationToken token = null;

                TableQuerySegment<Track> tableQueryResult = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                TableOperation op = TableOperation.Delete(tableQueryResult.FirstOrDefault());
                await table.ExecuteAsync(op);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get tracks by ownerId.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns>List of tracks by ownerId.</returns>
        internal static async Task<List<Track>> GetTracks(string ownerId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);

                // query tracks
                TableQuery<Track> query = new TableQuery<Track>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ownerId));

                List<Track> allTracks = new List<Track>();
                TableContinuationToken token = null;

                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = queryResponse.ContinuationToken;
                    allTracks.AddRange(queryResponse.Results);
                }
                while (token != null);

                return allTracks.OrderByDescending(t => t.Date_Created).ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Fetches the track based upon the requestKey.
        /// </summary>
        /// <param name="requestKey"></param>
        /// <returns></returns>
        internal static async Task<Track> GetTrackByRequestKey(string requestKey)
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

        internal static async void InsertOrIncrementTrackTag(TrackTag trackTag)
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

        internal static async Task<TrackTag> GetTrackTag(string trackId, string tag)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(TrackTagsTable);

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<TrackTag>(trackId, tag);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (TrackTag)retrievedResult.Result;
        }

        internal static async Task<ExtendedUser> CreateExtendedUser(ExtendedUser extendedUser)
        {
            try
            {
                // reference users table
                CloudTable table = tableClient.GetTableReference(ExtendedUsersTable);
                await table.CreateIfNotExistsAsync();

                // insert the user
                TableOperation op = TableOperation.Insert(extendedUser);
                var result = await table.ExecuteAsync(op);
                if (result == null)
                    return null;

                return extendedUser;
            }
            catch
            {
                throw;
            }
        }

        internal static async Task<ExtendedUser> GetExtendedUser(string userId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(ExtendedUsersTable);

                // query tracks
                TableQuery<ExtendedUser> rangeQuery = new TableQuery<ExtendedUser>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userId));

                TableContinuationToken token = null;

                TableQuerySegment<ExtendedUser> tableQueryResult =
                    await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                return tableQueryResult.FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        internal static async Task<ExtendedUser> UpdateExtendedUser(ExtendedUser user)
        {
            try
            {
                // reference users table
                CloudTable table = tableClient.GetTableReference(ExtendedUsersTable);
                await table.CreateIfNotExistsAsync();

                // insert the user
                TableOperation op = TableOperation.Replace(user);
                var result = await table.ExecuteAsync(op);
                if (result == null)
                    return null;

                return user;
            }
            catch
            {
                throw;
            }
        }

        internal static void AddMessageToQueue(string queueName, string messageBody)
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
