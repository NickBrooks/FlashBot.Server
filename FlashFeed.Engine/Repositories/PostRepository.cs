using FlashFeed.Engine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class PostRepository
    {
        // table storage stuff
        public static async Task<Post> InsertPost(PostSubmitDTO postDTO)
        {
            long now = Tools.ConvertToEpoch(DateTime.UtcNow);
            long countdown = Tools.GetCountdownFromDateTime(now);
            string id = countdown.ToString() + Guid.NewGuid().ToString();

            Post post = new Post(id, postDTO.track_id)
            {
                body = postDTO.body,
                url = postDTO.url,
                summary = postDTO.summary,
                date_created = now,
                track_name = postDTO.track_name,
                tags = string.Join(",", postDTO.tags),
                title = postDTO.title,
                type = postDTO.type,
                has_image = false
            };


            // TODO: process image
            if (Tools.ValidateUri(postDTO.image_url))
            {
                post.has_image = await ProcessImage(id, postDTO.image_url);
            }

            var result = await TableStorageRepository.InsertPost(post);

            if (result == null)
                return null;

            // shit hack to remove body for queue processing
            post.body = null;

            // add to queues for further processing
            TableStorageRepository.AddMessageToQueue("process-new-post-increment-track-tags", JsonConvert.SerializeObject(post));
            TableStorageRepository.AddMessageToQueue("process-new-post-add-to-cosmos", JsonConvert.SerializeObject(post));

            // check rate limit
            Random rnd = new Random();
            if (rnd.Next(1, 8) == 3)
            {
                TableStorageRepository.AddMessageToQueue("process-new-post-check-rate-limit", post.PartitionKey);
            }

            return post;
        }

        public static async Task<List<string>> GetPostIdsInTrack(string trackId)
        {
            return await TableStorageRepository.GetPostIdsInTrack(trackId);
        }

        public static async Task<PostReturnObject> GetPosts(PostQuery query)
        {
            List<PostQueryDTO> data = await TableStorageRepository.GetPosts(query);
            string continuation = data.Count > 1 ? data[data.Count - 1].date_created.ToString() : null;

            return new PostReturnObject()
            {
                continuation = continuation,
                count = data.Count,
                data = data
            };
        }

        public static void DeletePostFromTableStorage(Post postMeta)
        {
            TableStorageRepository.DeletePost(postMeta);
        }

        public static async Task<Post> GetPost(string trackId, string postId)
        {
            return await TableStorageRepository.GetPost(trackId, postId);
        }

        public static async Task<int> PostsLastHourCount(string trackId)
        {
            return await TableStorageRepository.GetPostCountSince(trackId, 60);
        }

        // cosmos stuff
        public static async Task<PostQueryDTO> InsertPostToCosmos(PostQueryDTO post)
        {
            return await (dynamic)CosmosRepository<PostQueryDTO>.CreateItemAsync(post);
        }

        public static async Task<PostReturnObject> QueryPosts(PostQuery query)
        {
            List<PostQueryDTO> data = await CosmosRepository<PostQueryDTO>.GetItemsSqlAsync(query.sql);
            string continuation = data.Count > 1 ? data[data.Count - 1].date_created.ToString() : null;

            return new PostReturnObject()
            {
                continuation = continuation,
                count = data.Count,
                data = data
            };
        }

        public static void DeletePostFromCosmos(string postId)
        {
            CosmosRepository<PostDTO>.DeleteItemAsync(postId);
        }

        // validation
        public static PostSubmitDTO ValidatePost(PostSubmitDTO postDTO)
        {
            if (postDTO == null) return null;
            if (postDTO.title == null) return null;
            if (postDTO.body == null && postDTO.url == null) return null;
            if (postDTO.tags.Count > 12) return null;

            // check valid URL
            if (!string.IsNullOrEmpty(postDTO.url) && !Tools.ValidateUri(postDTO.url))
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

        // images
        private static async Task<bool> ProcessImage(string postId, string imageUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(imageUrl);
                string contentType = webClient.ResponseHeaders["Content-Type"];

                // check correct content type
                if ("image/jpeg" != contentType)
                    return false;

                /// generate mini
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.CropSquare(48, input, output);

                        using (output)
                        {
                            await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), postId + "/thumb_mini", contentType);
                        }
                    }
                }

                // generate thumb
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.CropSquare(150, input, output);

                        using (output)
                        {
                            await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), postId + "/thumb", contentType);
                        }
                    }
                }

                // hero
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.GenerateHero(400, input, output);

                        using (output)
                        {
                            await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), postId + "/hero", contentType);
                        }
                    }
                }

                // resized
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.Resize(800, input, output);

                        using (output)
                        {
                            await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), postId + "/large", contentType);
                        }
                    }
                }

                return true;
            }
        }

        public static void DeleteImages(string postId)
        {
            BlobRepository.DeleteFolder(postId, BlobRepository.PostsContainer);
        }
    }
}
