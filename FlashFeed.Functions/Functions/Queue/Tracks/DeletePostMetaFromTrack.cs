using System;
using System.Collections.Generic;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class DeletePostMetaFromTrack
    {
        [FunctionName("DeletePostMetaFromTrack")]
        public static async void Run([QueueTrigger("delete-postmeta-from-track", Connection = "AzureWebJobsStorage")]string trackId, TraceWriter log)
        {
            List<Post> postsToDelete = await PostRepository.GetPostsInTrack(trackId);

            foreach (var post in postsToDelete)
            {
                PostRepository.DeletePostFromTableStorage(post);
            }

            log.Info($"C# Queue trigger function deleted track tags: {trackId}");
        }
    }
}