using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlashFeed.Engine.Models;

namespace FlashFeed.Engine.Repositories
{
    public class PostTableStorageRepository
    {
        public static async Task<Post> InsertPost(Post post)
        {
            return await TableStorageRepository.InsertPost(post);
        }

        public static async Task<List<Post>> GetPostsInTrack(string trackId)
        {
            return await TableStorageRepository.GetPostsInTrack(trackId);
        }

        public static void DeletePost(Post postMeta)
        {
            TableStorageRepository.DeletePost(postMeta);
        }

        public static async Task<Post> GetPost(string trackId, string postId)
        {
            return await TableStorageRepository.GetPost(trackId, postId);
        }

        public static int GetPostsLastHourAsync(string track_id)
        {
            return TableStorageRepository.QueryEntities<Post>(t => t.PartitionKey == track_id && t.Timestamp > DateTime.UtcNow.AddHours(-1), "posts").Count; ;
        }
    }
}
