using System;
using System.Collections.Generic;
using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.Queue
{
    public static class DeleteRequestMetaFromTrack
    {
        [FunctionName("DeleteRequestMetaFromTrack")]
        public static async void Run([QueueTrigger("delete-requestmeta-from-track", Connection = "AzureWebJobsStorage")]string trackId, TraceWriter log)
        {
            List<RequestTableStorage> requestsToDelete = await RequestTableStorageRepository.GetRequestsInTrack(trackId);

            foreach (var request in requestsToDelete)
            {
                RequestTableStorageRepository.DeleteRequest(request);
            }

            log.Info($"C# Queue trigger function deleted track tags: {trackId}");
        }
    }
}
