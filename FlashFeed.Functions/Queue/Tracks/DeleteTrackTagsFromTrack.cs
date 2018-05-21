using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;

namespace FlashBot.API.Queue.Tracks
{
    public static class DeleteTrackTagsFromTrack
    {
        [FunctionName("DeleteTrackTagsFromTrack")]
        public static async void Run([QueueTrigger("delete-tracktags-from-track", Connection = "TABLESTORAGE_CONNECTION")]CloudQueueMessage trackId, TraceWriter log)
        {
            List<TrackTag> listOfTrackTagsToDelete = await TrackTagRepository.GetTagsByTrack(trackId.AsString);

            foreach (TrackTag trackTag in listOfTrackTagsToDelete)
            {
                TrackTagRepository.DeleteTrackTag(trackTag);
            }

            log.Info($"C# Queue trigger function deleted track tags: {trackId.AsString}");
        }
    }
}
