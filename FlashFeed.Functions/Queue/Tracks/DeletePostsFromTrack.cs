using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;

namespace FlashFeed.Functions.Queue.Tracks
{
    public static class DeletePostsFromTrack
    {
        [FunctionName("DeletePostsFromTrack")]
        public static async void Run([QueueTrigger("delete-posts-from-track", Connection = "TABLESTORAGE_CONNECTION")]string trackId, TraceWriter log)
        {
            List<string> listOfPostsToDelete = await PostRepository.GetPostIdsInTrack(trackId);

            foreach (var postId in listOfPostsToDelete)
            {
                TableStorageRepository.AddMessageToQueue("process-delete-post", $"{trackId}.{postId}");
            }

            log.Info($"Added all posts in track to delete queue: {trackId}");
        }
    }
}
