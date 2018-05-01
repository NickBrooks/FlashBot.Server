using FlashFeed.Engine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class TableStorageRepository
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        private static readonly string TrackTagsTable = "tracktags";
        private static readonly string TracksTable = "tracks";
        private static readonly string ExtendedUsersTable = "extendedusers";
        private static readonly string PostsTable = "posts";

        /// <summary>
        /// Insert track into Table Storage.
        /// </summary>
        /// <param name="track"></param>
        /// <returns>Returns the inserted track.</returns>
        internal static async Task<TrackAuth> InsertTrackAuth(TrackAuth track)
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


        internal static List<TrackAuth> GetRateLimitedTracks()
        {
            return QueryEntities<TrackAuth>(t => t.rate_limit_exceeded == true, TracksTable);
        }

        internal static async void UpdateTrack(TrackAuth track)
        {
            try
            {
                // get the table
                CloudTable table = tableClient.GetTableReference(TracksTable);

                TableOperation op = TableOperation.InsertOrReplace(track);
                await table.ExecuteAsync(op);
            }
            catch (StorageException e)
            {
                throw;
            }
        }

        /// <summary>
        /// Get track from Table Storage.
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns>Returns the inserted track.</returns>
        internal static async Task<TrackAuth> GetTrack(string trackId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);

                // query tracks
                TableQuery<TrackAuth> rangeQuery = new TableQuery<TrackAuth>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, trackId));

                TableContinuationToken token = null;

                TableQuerySegment<TrackAuth> tableQueryResult =
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
                TableQuery<TrackAuth> rangeQuery = new TableQuery<TrackAuth>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, trackId));

                TableContinuationToken token = null;

                TableQuerySegment<TrackAuth> tableQueryResult = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                TableOperation op = TableOperation.Delete(tableQueryResult.FirstOrDefault());
                await table.ExecuteAsync(op);
            }
            catch
            {
                throw;
            }
        }

        internal static async void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            try
            {
                // get the table
                CloudTable table = tableClient.GetTableReference(TrackTagsTable);

                var exists = await GetTrackTag(trackTag.PartitionKey, trackTag.RowKey);

                if (exists != null)
                {
                    // update
                    exists.count++;
                    TableOperation op = TableOperation.Merge(exists);
                    await table.ExecuteAsync(op);
                }
                else
                {
                    // insert
                    trackTag.count = 1;
                    TableOperation op = TableOperation.InsertOrReplace(trackTag);
                    await table.ExecuteAsync(op);
                }
            }
            catch (StorageException e)
            {
                throw;
            }
        }

        internal static async Task<List<TrackTag>> GetTagsByTrack(string trackId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TrackTagsTable);

                // query tracks
                TableQuery<TrackTag> query = new TableQuery<TrackTag>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, trackId));

                List<TrackTag> trackTags = new List<TrackTag>();
                TableContinuationToken token = null;

                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = queryResponse.ContinuationToken;
                    trackTags.AddRange(queryResponse.Results);
                }
                while (token != null);

                return trackTags.OrderByDescending(t => t.count).ToList();
            }
            catch
            {
                throw;
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

        internal static async void DeleteTrackTag(TrackTag trackTag)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TrackTagsTable);

                TableOperation op = TableOperation.Delete(trackTag);
                await table.ExecuteAsync(op);
            }
            catch
            {
                throw;
            }
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

        internal static async Task<Post> InsertPost(Post post)
        {
            try
            {
                // reference users table
                CloudTable table = tableClient.GetTableReference(PostsTable);
                await table.CreateIfNotExistsAsync();

                // insert the user
                TableOperation op = TableOperation.Insert(post);
                var result = await table.ExecuteAsync(op);

                if (result == null)
                    return null;

                return post;
            }
            catch
            {
                throw;
            }
        }

        internal static async Task<Post> GetPost(string trackId, string postId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(PostsTable);

                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<Post>(trackId, postId);

                // Execute the retrieve operation.
                TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
                return (Post)retrievedResult.Result;
            }
            catch
            {
                throw;
            }
        }

        internal static List<PostQueryDTO> GetPosts(PostQuery postQuery, int count = 30)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(PostsTable);

                // set track condition
                long countdownTime = Convert.ToInt64(postQuery.continuation_time) - 1;
                string trackPredicate = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, postQuery.track_id);
                string continuationPredicate = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, Tools.GetCountdownFromDateTime(countdownTime).ToString());

                // continuation, yay or nay
                string predicate = postQuery.continuation_time == null ? trackPredicate : TableQuery.CombineFilters(trackPredicate, TableOperators.And, continuationPredicate);

                // add the created clause
                TableQuery<Post> query = new TableQuery<Post>().Where(predicate).Select(new string[] { "PartitionKey", "RowKey", "track_name", "date_created", "tags", "type", "title", "summary", "url", "has_image" }).Take(count);

                List<PostQueryDTO> postQueryDTO = new List<PostQueryDTO>();

                foreach (var post in table.ExecuteQuery(query))
                {
                    postQueryDTO.Add(Tools.ConvertPostToPostQueryDTO(post));
                }

                return postQueryDTO;
            }
            catch
            {
                throw;
            };
        }

        internal static List<string> GetPostIdsInTrack(string trackId)
        {
            // reference track table
            CloudTable table = tableClient.GetTableReference(PostsTable);

            TableQuery<Post> projectionQuery = new TableQuery<Post>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, trackId)).Select(new string[] { "RowKey" });

            List<string> idsInTrack = new List<string>();

            foreach (Post post in table.ExecuteQuery(projectionQuery))
            {
                idsInTrack.Add(post.RowKey);
            }

            return idsInTrack;
        }

        internal static int GetPostCountSince(string trackId, int minutes)
        {
            // reference track table
            CloudTable table = tableClient.GetTableReference(PostsTable);

            long epochTime = Tools.GetCountdownFromDateTime(DateTime.UtcNow.AddMinutes(minutes * -1));

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, trackId),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, epochTime.ToString()))).Select(new string[] { "RowKey" });

            return table.ExecuteQuery(query).Count();
        }

        internal static async void DeletePost(Post postMeta)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(PostsTable);

                TableOperation op = TableOperation.Delete(postMeta);
                await table.ExecuteAsync(op);
            }
            catch
            {
                throw;
            }
        }

        public static async void AddMessageToQueue(string queueName, string messageBody)
        {
            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist.
            await queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(messageBody);
            await queue.AddMessageAsync(message);
        }

        // https://goo.gl/gyaX67
        internal static List<T> QueryEntities<T>(Expression<Func<T, bool>> criteria, string tableName) where T : ITableEntity, new()
        {
            // reference table
            CloudTable table = tableClient.GetTableReference(tableName);

            // table, in this case, is my `CloudTable` instance
            List<T> results = table.CreateQuery<T>().Where(criteria).ToList();

            return results;
        }
    }
}
