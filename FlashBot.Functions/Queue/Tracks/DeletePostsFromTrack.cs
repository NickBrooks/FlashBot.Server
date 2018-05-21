using FlashBot.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;

namespace FlashBot.API.Queue.Tracks
{
    public static class DeletePostsFromTrack
    {
        [FunctionName("DeletePostsFromTrack")]
        public static async void Run([QueueTrigger("delete-posts-from-track", Connection = "QUEUESTORAGE_CONNECTION")]CloudQueueMessage trackId, TraceWriter log)
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
