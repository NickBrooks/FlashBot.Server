using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.Queue
{
    public static class DeleteTrackTagsFromTrack
    {
        [FunctionName("DeleteTrackTagsFromTrack")]
        public static async void Run([QueueTrigger("delete-tracktags-from-track", Connection = "AzureWebJobsStorage")]string trackId, TraceWriter log)
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
