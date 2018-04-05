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
            List<RequestMeta> listOfRequestMetaToDelete = await RequestMetaRepository.GetListOfRequestMetaInTrackAsync(trackId);

            foreach (var requestMeta in listOfRequestMetaToDelete)
            {
                RequestMetaRepository.DeleteRequestMeta(requestMeta);
            }

            log.Info($"C# Queue trigger function deleted track tags: {trackId}");
        }
    }
}
