using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlashFeed.Engine.Models;

namespace FlashFeed.Engine.Repositories
{
    public class PostTableStorageRepository
    {
        public static async Task<PostTableStorage> InsertPost(PostTableStorage post)
        {
            return await TableStorageRepository.InsertPost(post);
        }

        public static async Task<List<PostTableStorage>> GetPostsInTrack(string trackId)
        {
            return await TableStorageRepository.GetPostsInTrack(trackId);
        }

        public static void DeletePost(PostTableStorage postMeta)
        {
            TableStorageRepository.DeletePost(postMeta);
        }

        public static async Task<PostTableStorage> GetPost(string trackId, string postId)
        {
            return await TableStorageRepository.GetPost(trackId, postId);
        }

        public static int GetPostsLastHourAsync(string track_id)
        {
            return TableStorageRepository.QueryEntities<PostTableStorage>(t => t.PartitionKey == track_id && t.Timestamp > DateTime.UtcNow.AddHours(-1), "posts").Count; ;
        }
    }
}
