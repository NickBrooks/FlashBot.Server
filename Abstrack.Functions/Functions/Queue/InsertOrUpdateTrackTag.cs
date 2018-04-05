using Abstrack.Engine.Models;
using Abstrack.Functions.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Abstrack.Functions.Functions.Queue
{
    public static class InsertOrUpdateTrackTag
    {
        [FunctionName("InsertOrUpdateTrackTag")]
        public static void Run([QueueTrigger("process-tracktags", Connection = "AzureWebJobsStorage")]string queueItem, TraceWriter log)
        {
            TrackTagsQueueItem trackTagsQueueItem = JsonConvert.DeserializeObject<TrackTagsQueueItem>(queueItem);

            // check if they exist, if not add
            foreach (var tag in trackTagsQueueItem.Tags)
            {
                TableStorageRepository.InsertOrIncrementTrackTag(new TrackTag(trackTagsQueueItem.Track_Id, tag));
            }

            log.Info($"C# Queue trigger function processed: {queueItem}");
        }
    }
}
