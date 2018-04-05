using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Abstrack.Functions.Functions.Queue
{
    public static class ProcessNewRequest
    {
        [FunctionName("ProcessNewRequest")]
        public static void Run([QueueTrigger("process-new-request", Connection = "AzureWebJobsStorage")]string queueItem, TraceWriter log)
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

            // do rate limit

            log.Info($"Post processing completed for request: {request.id}");
        }
    }
}
