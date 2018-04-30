using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class DeletePostsFromTrack
    {
        [FunctionName("DeletePostsFromTrack")]
        public static void Run([QueueTrigger("delete-posts-from-track", Connection = "AzureWebJobsStorage")]string trackId, TraceWriter log)
        {
            List<string> listOfPostsToDelete = PostRepository.GetPostIdsInTrack(trackId);

            foreach (var postId in listOfPostsToDelete)
            {
                TableStorageRepository.AddMessageToQueue("process-delete-post", $"{trackId}.{postId}");
            }

            log.Info($"Added all posts in track to delete queue: {trackId}");
        }
    }
}
