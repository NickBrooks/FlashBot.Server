using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class DeleteTrackTagsFromTrack
    {
        [FunctionName("DeleteTrackTagsFromTrack")]
        public static async void Run([QueueTrigger("delete-tracktags-from-track", Connection = "TABLESTORAGE_CONNECTION")]string trackId, TraceWriter log)
        {
            var listOfTrackTagsToDelete = await TrackTagRepository.GetTagsByTrack(trackId);

            foreach (var trackTag in listOfTrackTagsToDelete)
            {
                TrackTagRepository.DeleteTrackTag(trackTag);
            }

            log.Info($"C# Queue trigger function deleted track tags: {trackId}");
        }
    }
}
