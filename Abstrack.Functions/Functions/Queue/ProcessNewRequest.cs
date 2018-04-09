using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Abstrack.Functions.Functions.Queue
{
    public static class ProcessNewRequest
    {
        [FunctionName("ProcessNewRequest")]
        public static async void Run([QueueTrigger("process-new-request", Connection = "AzureWebJobsStorage")]string queueItem, TraceWriter log)
        {
            RequestTableStorage request = JsonConvert.DeserializeObject<RequestTableStorage>(queueItem);

            // add tags to tracktag list
            foreach (var tag in request.tags.Split(',').ToList())
            {
                TrackTagRepository.InsertOrIncrementTrackTag(new TrackTag(request.PartitionKey, tag));
            }

            // check rate limit
            Random rnd = new Random();
            if (rnd.Next(1, 8) == 3)
            {
                var track = await TrackRepository.GetTrack(request.PartitionKey);
                int requestsLastHour = RequestTableStorageRepository.GetRequestsLastHourAsync(request.PartitionKey);

                if (requestsLastHour > track.rate_limit)
                {
                    track.rate_limit_exceeded = true;
                    TrackRepository.UpdateTrack(track);
                }
            }

            log.Info($"Post processing completed for request: {request.RowKey}");
        }
    }
}
