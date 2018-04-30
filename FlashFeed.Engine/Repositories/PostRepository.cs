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
        public static async Task<Post> InsertPost(PostDTO postDTO)
        {
            DateTime now = DateTime.UtcNow;
            long countdown = Tools.GetCountdownFromDateTime(now);
            string id = countdown.ToString() + Guid.NewGuid().ToString();

            Post post = new Post(id, postDTO.track_id)
            {
                body = postDTO.body,
                url = postDTO.url,
                summary = postDTO.summary,
                date_created = now,
                tags = string.Join(",", postDTO.tags),
                title = postDTO.title,
                type = postDTO.type
            };

            var result = await PostTableStorageRepository.InsertPost(post);

            if (result == null)
                return null;

            // shit hack to remove body for queue processing
            post.body = null;

            // add to queues for further processing
            TableStorageRepository.AddMessageToQueue("process-new-post-increment-track-tags", JsonConvert.SerializeObject(post));

            // check rate limit
            //Random rnd = new Random();
            //if (rnd.Next(1, 8) == 3)
            //{
            TableStorageRepository.AddMessageToQueue("process-new-post-check-rate-limit", post.PartitionKey);
            //}

            return post;
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
            if (postDTO.title == null) return null;
            if (postDTO.body == null && postDTO.url == null) return null;
            if (postDTO.tags.Count > 12) return null;

            // check valid URL
            Uri uriResult;
            bool validURI = Uri.TryCreate(postDTO.url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!string.IsNullOrEmpty(postDTO.url) && !validURI)
                return null;

            // if has URL
            postDTO.type = string.IsNullOrEmpty(postDTO.url) ? "post" : "url";
            postDTO.body = postDTO.type == "post" ? postDTO.body : "";

            // check for provided summary
            if (!string.IsNullOrEmpty(postDTO.summary))
                postDTO.summary = postDTO.summary.Length > 140 ? postDTO.summary.Substring(0, 140) : postDTO.summary;

            postDTO.title = postDTO.title.Length > 80 ? postDTO.title.Substring(0, 80) : postDTO.title;
            postDTO.body = postDTO.body.Length > 10000 ? postDTO.body.Substring(0, 10000) : postDTO.body;
            postDTO.summary = string.IsNullOrEmpty(postDTO.summary) ? Tools.GenerateSummary(postDTO.body) : postDTO.summary;
            postDTO.tags = Tools.ValidateTags(postDTO.tags);

            return postDTO;
        }
    }
}
