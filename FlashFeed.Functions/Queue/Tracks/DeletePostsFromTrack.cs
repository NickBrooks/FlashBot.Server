using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;

namespace FlashFeed.Functions.Queue.Tracks
{
    public static class DeletePostsFromTrack
    {
        [FunctionName("DeletePostsFromTrack")]
        public static async void Run([QueueTrigger("delete-posts-from-track", Connection = "TABLESTORAGE_CONNECTION")]CloudQueueMessage trackId, TraceWriter log)
        {
            List<string> listOfPostsToDelete = await PostRepository.GetPostIdsInTrack(trackId.AsString);

            foreach (var postId in listOfPostsToDelete)
            {
                TableStorageRepository.AddMessageToQueue("process-delete-post", $"{trackId.AsString}.{postId}");
            }

            log.Info($"Added all posts in track to delete queue: {trackId.AsString}");
        }
    }
}
