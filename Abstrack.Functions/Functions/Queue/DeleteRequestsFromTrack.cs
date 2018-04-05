using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.Queue
{
    public static class DeleteRequestsFromTrack
    {
        [FunctionName("DeleteRequestsFromTrack")]
        public static void Run([QueueTrigger("deleterequestsfromtrack", Connection = "AzureWebJobsStorage")]string trackID, TraceWriter log)
        {
            
            log.Info($"C# Queue trigger function deleted: {trackID}");
        }
    }
}
