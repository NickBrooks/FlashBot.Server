using FlashFeed.Engine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class PostRepository
    {
        public static async Task<PostDTO> InsertPost(PostDTO postDTO)
        {
            // cosmos insertion
            PostCosmos postCosmos = new PostCosmos()
            {
                date_created = postDTO.date_created,
                summary = postDTO.summary,
                tags = postDTO.tags,
                title = postDTO.title,
                track_id = postDTO.track_id
            };

            var cosmosResult = await (dynamic)CosmosRepository<PostCosmos>.CreateItemAsync(postCosmos);

            if (cosmosResult == null)
                return null;

            // table storage insertion
            PostTableStorage postTableStorage = new PostTableStorage(cosmosResult.id, postDTO.track_id)
            {
                summary = postDTO.summary,
                tags = string.Join(",", postDTO.tags),
                title = postDTO.title,
                body = postDTO.body,
                date_created = postDTO.date_created,
                _ts = cosmosResult._ts
            };

            await PostTableStorageRepository.InsertPost(postTableStorage);

            // add to queue for further processing
            TableStorageRepository.AddMessageToQueue("process-new-post", JsonConvert.SerializeObject(postTableStorage));

            return cosmosResult;
        }

        public static async Task<PostDTO> GetPost(string trackId, string postId)
        {
            var result = await PostTableStorageRepository.GetPost(trackId, postId);

            if (result == null)
                return null;

            return new PostDTO()
            {
                id = postId,
                track_id = trackId,
                date_created = result.date_created,
                title = result.title,
                body = result.body,
                summary = result.summary,
                tags = result.tags.Split(',').ToList()
            };
        }

        public static async Task<PostReturnObject> GetPosts(string trackId, PostQuery query, string continuationToken = null)
        {
            ContinuationToken token = null;

            if (continuationToken != null)
                token = await ContinuationTokenRepository.GetContinuationToken(trackId, continuationToken);

            var result = await CosmosRepository<PostQueryDTO>.GetItemsSqlWithPagingAsync(query.sql, 50, token?.Continuation_Token == null ? null : token.Continuation_Token);

            // generate continuationToken
            string newToken = null;
            if (!string.IsNullOrEmpty(result.continuationToken))
            {
                newToken = AuthRepository.GenerateSHA256(result.continuationToken, DateTime.UtcNow.ToString());
                ContinuationTokenRepository.InsertContinuationToken(new ContinuationToken(trackId, newToken)
                {
                    Continuation_Token = result.continuationToken,
                });
            }

            if (token != null)
                ContinuationTokenRepository.DeleteContinuationToken(token);

            return new PostReturnObject()
            {
                continuation_token = newToken,
                count = result.results.Count(),
                data = result.results
            };
        }

        public static async Task<List<PostDTO>> GetListOfPostIdsInTrack(string trackId)
        {
            return await CosmosRepository<PostDTO>.GetItemsSqlAsync($"SELECT r.id FROM r WHERE r.track_id = '{trackId}'");
        }

        public static async void DeletePost(string postId)
        {
            await CosmosRepository<PostDTO>.DeleteItemAsync(postId);
        }

        public static PostDTO ValidatePost(PostDTO postDTO)
        {
            if (postDTO == null) return null;
            if (postDTO.body == null) return null;
            if (postDTO.tags.Count > 12) return null;

            // check for provided summary
            if (!string.IsNullOrEmpty(postDTO.summary))
                postDTO.summary = postDTO.summary.Length > 140 ? postDTO.summary.Substring(0, 140) : postDTO.summary;

            postDTO.title = postDTO.title.Length > 80 ? postDTO.title.Substring(0, 80) : postDTO.title;
            postDTO.body = postDTO.body.Length > 10000 ? postDTO.body.Substring(0, 10000) : postDTO.body;
            postDTO.summary = string.IsNullOrEmpty(postDTO.summary) ? Tools.GenerateSummary(postDTO.body) : postDTO.summary;
            postDTO.tags = Tools.ValidateTags(postDTO.tags);
            postDTO.date_created = DateTime.UtcNow;

            return postDTO;
        }
    }
}
