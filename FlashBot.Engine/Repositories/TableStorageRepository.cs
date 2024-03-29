﻿using FlashBot.Engine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FlashBot.Engine.Repositories
{
    public class TableStorageRepository
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLESTORAGE_CONNECTION"));

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        private static readonly string TrackTagsTable = "tracktags";
        private static readonly string TracksTable = "tracks";
        private static readonly string TrackFollowsTable = "trackfollows";
        private static readonly string UserFollowsTable = "userfollows";
        private static readonly string ExtendedUsersTable = "extendedusers";
        private static readonly string PostsTable = "posts";
        private static readonly string RefreshTokensTable = "refreshtokens";

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

        internal static async Task<List<TrackAuth>> GetRateLimitedTracks()
        {
            // reference track table
            CloudTable table = tableClient.GetTableReference(PostsTable);
            await table.CreateIfNotExistsAsync();

            TableQuery<TrackAuth> query = new TableQuery<TrackAuth>().Where(
                    TableQuery.GenerateFilterCondition("rate_limit_exceeded", QueryComparisons.Equal, "true")).Select(new string[] { "RowKey", "rate_limit" });

            List<TrackAuth> tracks = new List<TrackAuth>();
            TableContinuationToken token = null;

            do
            {
                var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                token = queryResponse.ContinuationToken;
                tracks.AddRange(queryResponse.Results);
            }
            while (token != null);

            return tracks;
        }

        internal static async void UpdateTrack(TrackAuth track)
        {
            try
            {
                // get the table
                CloudTable table = tableClient.GetTableReference(TracksTable);
                await table.CreateIfNotExistsAsync();

                TableOperation op = TableOperation.InsertOrReplace(track);
                await table.ExecuteAsync(op);
            }
            catch (StorageException e)
            {
                throw;
            }
        }

        internal static async Task<List<TrackAuth>> GetTracksByOwnerId(string ownerId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);
                await table.CreateIfNotExistsAsync();

                // query tracks
                TableQuery<TrackAuth> query = new TableQuery<TrackAuth>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ownerId));

                List<TrackAuth> tracks = new List<TrackAuth>();
                TableContinuationToken token = null;

                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = queryResponse.ContinuationToken;
                    tracks.AddRange(queryResponse.Results);
                }
                while (token != null);

                return tracks.OrderByDescending(t => t.subscribers).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
                await table.CreateIfNotExistsAsync();

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
        internal static async Task<bool> DeleteTrack(string trackId)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TracksTable);
                await table.CreateIfNotExistsAsync();

                // query tracks
                TableQuery<TrackAuth> rangeQuery = new TableQuery<TrackAuth>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, trackId));

                TableContinuationToken token = null;

                TableQuerySegment<TrackAuth> tableQueryResult = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                TableOperation op = TableOperation.Delete(tableQueryResult.FirstOrDefault());
                await table.ExecuteAsync(op);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static async void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            try
            {
                // get the table
                CloudTable table = tableClient.GetTableReference(TrackTagsTable);
                await table.CreateIfNotExistsAsync();

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
                await table.CreateIfNotExistsAsync();

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
            await table.CreateIfNotExistsAsync();

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
                await table.CreateIfNotExistsAsync();

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
                await table.CreateIfNotExistsAsync();

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
                await table.CreateIfNotExistsAsync();

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

        internal static async Task<List<PostQueryDTO>> GetPosts(PostQuery postQuery, int count = 30)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(PostsTable);
                await table.CreateIfNotExistsAsync();

                // set track condition
                long countdownTime = Convert.ToInt64(postQuery.continuation) - 1;
                string trackPredicate = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, postQuery.track_id);
                string continuationPredicate = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, Tools.GetCountdownFromDateTime(countdownTime).ToString());

                // continuation, yay or nay
                string predicate = postQuery.continuation == null ? trackPredicate : TableQuery.CombineFilters(trackPredicate, TableOperators.And, continuationPredicate);

                // add the created clause
                TableQuery<Post> query = new TableQuery<Post>().Where(predicate).Select(new string[] { "PartitionKey", "RowKey", "track_name", "date_created", "tags", "type", "title", "summary", "url", "has_image" }).Take(count);

                List<Post> items = new List<Post>();
                TableContinuationToken token = null;

                do
                {
                    if (items.Count >= count)
                        break;

                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = queryResponse.ContinuationToken;
                    items.AddRange(queryResponse.Results);
                }
                while (token != null);

                List<PostQueryDTO> posts = new List<PostQueryDTO>();
                foreach (var post in items)
                {
                    posts.Add(Tools.ConvertPostToPostQueryDTO(post));
                }

                return posts.OrderByDescending(p => p.date_created).ToList();
            }
            catch
            {
                throw;
            };
        }

        internal static async Task<List<string>> GetPostIdsInTrack(string trackId)
        {
            // reference track table
            CloudTable table = tableClient.GetTableReference(PostsTable);
            await table.CreateIfNotExistsAsync();

            TableQuery<Post> query = new TableQuery<Post>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, trackId)).Select(new string[] { "RowKey" });

            List<Post> posts = new List<Post>();
            TableContinuationToken token = null;

            do
            {
                var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                token = queryResponse.ContinuationToken;
                posts.AddRange(queryResponse.Results);
            }
            while (token != null);

            List<string> idList = new List<string>();

            foreach (var post in posts)
            {
                idList.Add(post.RowKey);
            }

            return idList;
        }

        internal static async Task<int> GetPostCountSince(string trackId, int minutes)
        {
            // reference track table
            CloudTable table = tableClient.GetTableReference(PostsTable);
            await table.CreateIfNotExistsAsync();

            long epochTime = Tools.GetCountdownFromDateTime(DateTime.UtcNow.AddMinutes(minutes * -1));

            TableQuery<Post> query = new TableQuery<Post>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, trackId),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, epochTime.ToString()))).Select(new string[] { "RowKey" });

            List<Post> posts = new List<Post>();
            TableContinuationToken token = null;

            do
            {
                var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                token = queryResponse.ContinuationToken;
                posts.AddRange(queryResponse.Results);
            }
            while (token != null);

            return posts.Count;
        }

        internal static async void DeletePost(Post postMeta)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(PostsTable);
                await table.CreateIfNotExistsAsync();

                TableOperation op = TableOperation.Delete(postMeta);
                await table.ExecuteAsync(op);
            }
            catch
            {
                throw;
            }
        }

        // follows
        internal static async Task<List<TrackFollowTableEntity>> GetTrackFollows(string trackId, Enums.FollowMode followMode = Enums.FollowMode.Feed)
        {
            try
            {
                // reference table
                CloudTable table = tableClient.GetTableReference(TrackFollowsTable);
                await table.CreateIfNotExistsAsync();

                // predicates
                string partitionPredicate = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, trackId);
                string feedPredicate = TableQuery.GenerateFilterCondition(followMode == Enums.FollowMode.Feed ? "feed_follow_type" : "notifications_follow_type", QueryComparisons.NotEqual, "none");

                // query track subscriptions
                TableQuery<TrackFollowTableEntity> query = new TableQuery<TrackFollowTableEntity>().Where(TableQuery.CombineFilters(partitionPredicate, TableOperators.And, feedPredicate));

                List<TrackFollowTableEntity> follows = new List<TrackFollowTableEntity>();
                TableContinuationToken token = null;

                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = queryResponse.ContinuationToken;
                    follows.AddRange(queryResponse.Results);
                }
                while (token != null);

                return follows;
            }
            catch
            {
                throw;
            }
        }

        internal static async Task<List<UserFollowTableEntity>> GetUserFollows(string userId)
        {
            try
            {
                // reference table
                CloudTable table = tableClient.GetTableReference(UserFollowsTable);
                await table.CreateIfNotExistsAsync();

                // query track subscriptions
                TableQuery<UserFollowTableEntity> query = new TableQuery<UserFollowTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

                List<UserFollowTableEntity> follows = new List<UserFollowTableEntity>();
                TableContinuationToken token = null;

                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = queryResponse.ContinuationToken;
                    follows.AddRange(queryResponse.Results);
                }
                while (token != null);

                return follows;
            }
            catch
            {
                throw;
            }
        }

        internal static async Task<TrackFollowTableEntity> GetTrackFollow(string trackId, string userId)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(TrackFollowsTable);
            await table.CreateIfNotExistsAsync();

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<TrackFollowTableEntity>(trackId, userId);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (TrackFollowTableEntity)retrievedResult.Result;
        }

        internal static async Task<UserFollowTableEntity> GetUserFollow(string userId, string trackId)
        {
            // get the table
            CloudTable table = tableClient.GetTableReference(UserFollowsTable);
            await table.CreateIfNotExistsAsync();

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<UserFollowTableEntity>(userId, trackId);

            // Execute the retrieve operation.
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            return (UserFollowTableEntity)retrievedResult.Result;
        }

        internal static async void InsertOrReplaceTrackFollow(TrackFollowTableEntity trackFollow)
        {
            // reference users table
            CloudTable table = tableClient.GetTableReference(TrackFollowsTable);
            await table.CreateIfNotExistsAsync();

            // insert the user
            TableOperation op = TableOperation.InsertOrReplace(trackFollow);
            await (dynamic)table.ExecuteAsync(op);
        }

        internal static async void InsertOrReplaceUserFollow(UserFollowTableEntity userFollowTableEntity)
        {
            // reference users table
            CloudTable table = tableClient.GetTableReference(UserFollowsTable);
            await table.CreateIfNotExistsAsync();

            // insert the user
            TableOperation op = TableOperation.InsertOrReplace(userFollowTableEntity);
            await (dynamic)table.ExecuteAsync(op);
        }

        internal static async Task<bool> DeleteTrackFollow(TrackFollowTableEntity trackFollow)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(TrackFollowsTable);
                await table.CreateIfNotExistsAsync();

                TableOperation op = TableOperation.Delete(trackFollow);
                var result = await table.ExecuteAsync(op);

                return result == null ? false : true;
            }
            catch
            {
                return false;
            }
        }

        internal static async Task<bool> DeleteUserFollow(UserFollowTableEntity userFollow)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(UserFollowsTable);
                await table.CreateIfNotExistsAsync();

                TableOperation op = TableOperation.Delete(userFollow);
                var result = await table.ExecuteAsync(op);

                return result == null ? false : true;
            }
            catch
            {
                return false;
            }
        }

        // refresh tokens
        internal static async Task InsertRefreshToken(RefreshToken refreshToken)
        {
            try
            {
                // reference tokens table
                CloudTable table = tableClient.GetTableReference(RefreshTokensTable);
                await table.CreateIfNotExistsAsync();

                // insert the token
                TableOperation op = TableOperation.Insert(refreshToken);
                await table.ExecuteAsync(op);
            }
            catch
            {
                throw;
            }
        }

        internal static async Task<RefreshToken> GetRefreshToken(string userId, string refreshToken)
        {
            try
            {
                // reference refreshToken table
                CloudTable table = tableClient.GetTableReference(RefreshTokensTable);
                await table.CreateIfNotExistsAsync();

                // Execute the retrieve operation.
                TableOperation retrieveOperation = TableOperation.Retrieve<RefreshToken>(userId, refreshToken);
                TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
                return (RefreshToken)retrievedResult.Result;
            }
            catch
            {
                throw;
            }
        }

        internal static async void DeleteRefreshToken(RefreshToken refreshToken)
        {
            try
            {
                // reference track table
                CloudTable table = tableClient.GetTableReference(RefreshTokensTable);
                await table.CreateIfNotExistsAsync();

                TableOperation op = TableOperation.Delete(refreshToken);
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
            await queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(messageBody);
            await queue.AddMessageAsync(message);
        }

        public static async Task AddMessageToQueueAsync(string queueName, string messageBody)
        {
            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(messageBody);
            queue.AddMessageAsync(message).Wait();
        }
    }
}
