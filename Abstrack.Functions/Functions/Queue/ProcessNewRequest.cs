using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;

namespace Abstrack.Functions.Functions.Queue
{
    public static class ProcessNewRequest
    {
        [FunctionName("ProcessNewRequest")]
        public static async void Run([QueueTrigger("process-new-request", Connection = "AzureWebJobsStorage")]string queueItem, TraceWriter log)
        {
            Request request = JsonConvert.DeserializeObject<Request>(queueItem);

            // add tags to tracktag list
            foreach (var tag in request.tags)
            {
                TrackTagRepository.InsertOrIncrementTrackTag(new TrackTag(request.track_id, tag));
            }

            // add request to table storage
            RequestMetaRepository.InsertRequestMeta(new RequestMeta(request.track_id, request.id)
            {
                Date_Created = request.date_created
            });

            // check rate limit
            Random rnd = new Random();
            if (rnd.Next(1, 8) == 3)
            {
                var track = await TrackRepository.GetTrack(request.track_id);
                int requestsLastHour = RequestMetaRepository.GetRequestsLastHourAsync(request.track_id);

                if (requestsLastHour > track.Rate_Limit)
                {
                    track.Rate_Limit_Exceeded = true;
                    TrackRepository.UpdateTrack(track);
                }
            }

            log.Info($"Post processing completed for request: {request.id}");
        }
    }
}
