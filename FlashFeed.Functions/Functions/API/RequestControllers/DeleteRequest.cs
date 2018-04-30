using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlashFeed.Functions.Functions.API.RequestControllers
{
    public static class DeleteRequest
    {
        [FunctionName("DeleteRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "track/{trackId}/request/{requestId}")]HttpRequestMessage req, string trackId, string requestId, TraceWriter log)
        {
            try
            {
                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, Tools.GetHeaderValue(req.Headers, "X-Track-Key"), Tools.GetHeaderValue(req.Headers, "X-Track-Secret")))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get request
                RequestTableStorage request = await RequestTableStorageRepository.GetRequest(trackId, requestId);
                if (request == null || track.RowKey != request.PartitionKey)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // delete the request
                RequestTableStorageRepository.DeleteRequest(request);
                RequestRepository.DeleteRequest(request.RowKey);

                // return response
                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
