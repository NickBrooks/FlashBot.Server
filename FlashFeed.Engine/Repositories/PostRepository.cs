﻿using FlashFeed.Engine.Models;
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
        public static async Task<Post> InsertPostToTableStorage(PostSubmitDTO postDTO)
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
                has_image = null
            };

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

        public static List<string> GetPostIdsInTrack(string trackId)
        {
            return TableStorageRepository.GetPostIdsInTrack(trackId);
        }

        public static PostReturnObject GetPosts(PostQuery query)
        {
            List<PostQueryDTO> data = TableStorageRepository.GetPosts(query);
            string continuation_time = data.Count > 1 ? data[data.Count - 1].date_created.ToString() : null;

            return new PostReturnObject()
            {
                continuation_time = continuation_time,
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

        public static int PostsLastHourCount(string trackId)
        {
            return TableStorageRepository.GetPostCountSince(trackId, 60);
        }

        // cosmos stuff
        public static async Task<PostQueryDTO> InsertPostToCosmos(PostQueryDTO post)
        {
            return await (dynamic)CosmosRepository<PostQueryDTO>.CreateItemAsync(post);
        }

        public static async Task<PostReturnObject> QueryPosts(PostQuery query)
        {
            CosmosQueryPagingResults<PostQueryDTO> result = await CosmosRepository<PostQueryDTO>.GetItemsSqlWithPagingAsync(query.sql);
            List<PostQueryDTO> data = result.results.ToList();
            string continuation_time = data.Count > 1 ? data[data.Count - 1].date_created.ToString() : null;

            return new PostReturnObject()
            {
                continuation_time = continuation_time,
                count = data.Count,
                data = data
            };
        }

        public static async void DeletePostFromCosmos(string postId)
        {
            await CosmosRepository<PostDTO>.DeleteItemAsync(postId);
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
        private static async Task<string> ProcessImage(string postId, string imageUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(imageUrl);
                string contentType = webClient.ResponseHeaders["Content-Type"];

                // check correct content type
                if (!new[] { "image/jpeg", "image/png" }.Any(s => s == contentType))
                    return null;

                // generate small thumb
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.CropSquare(32, input, output);

                        using (var fetchedImage = Image.FromStream(output))
                        {
                            await BlobRepository.UploadFileAsync(Images.ImageToByteArray(fetchedImage), postId + "/thumb_mini", contentType);
                        }
                    }
                }

                // generate medium thumb
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.CropSquare(150, input, output);

                        using (var fetchedImage = Image.FromStream(output))
                        {
                            await BlobRepository.UploadFileAsync(Images.ImageToByteArray(fetchedImage), postId + "/thumb", contentType);
                        }
                    }
                }

                // generate hero
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.GenerateHero(400, input, output);

                        using (var fetchedImage = Image.FromStream(output))
                        {
                            await BlobRepository.UploadFileAsync(Images.ImageToByteArray(fetchedImage), postId + "/hero", contentType);
                        }
                    }
                }

                // save a fullish size
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.ResizeToMax(800, input, output);

                        using (var fetchedImage = Image.FromStream(output))
                        {
                            await BlobRepository.UploadFileAsync(Images.ImageToByteArray(fetchedImage), postId + "/full", contentType);
                        }
                    }
                }

                switch (contentType)
                {
                    case "image/jpeg":
                        return "jpeg";
                    case "image/png":
                        return "png";
                    default:
                        return null;
                }
            }
        }

        public static void DeleteImages(string postId, string extension)
        {
            BlobRepository.DeleteFile($"{postId}/thumb_mini.{extension}");
            BlobRepository.DeleteFile($"{postId}/thumb.{extension}");
            BlobRepository.DeleteFile($"{postId}/hero.{extension}");
            BlobRepository.DeleteFile($"{postId}/full.{extension}");
        }
    }
}
