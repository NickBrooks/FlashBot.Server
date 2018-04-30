using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlashFeed.Engine.Models;
using Microsoft.WindowsAzure.Storage.Table;

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

        public static int PostsLastHourCount(string trackId)
        {
            return TableStorageRepository.GetPostCountSince(trackId, 60);
        }
    }
}
