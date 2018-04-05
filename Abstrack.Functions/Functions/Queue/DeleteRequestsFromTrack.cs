using System;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.Queue
{
    public static class DeleteRequestsFromTrack
    {
        [FunctionName("DeleteRequestsFromTrack")]
        public static async void Run([QueueTrigger("delete-requests-from-track", Connection = "AzureWebJobsStorage")]string trackId, TraceWriter log)
        {
            var listOfRequestsToDelete = await RequestRepository.GetListOfRequestIdsInTrack(trackId);

            foreach (var request in listOfRequestsToDelete)
            {
                RequestRepository.DeleteRequest(request.id);
            }

            log.Info($"C# Queue trigger function deleted: {trackId}");
        }
    }
}
