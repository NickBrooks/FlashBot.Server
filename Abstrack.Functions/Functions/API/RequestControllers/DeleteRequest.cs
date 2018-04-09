using Abstrack.Engine;
using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abstrack.Functions.Functions.API.RequestControllers
{
    public static class DeleteRequest
    {
        [FunctionName("DeleteRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "request/{requestId}")]HttpRequestMessage req, string requestId, TraceWriter log)
        {
            try
            {
                // check request key provided
                if (!Tools.IsValidGuid(requestId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get request key to do checks
                var requestKey = Tools.GetHeaderValue(req.Headers, "X-Request-Key");
                if (requestKey == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get track
                Track track = await TrackRepository.GetTrackByRequestKey(requestKey);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get requestmeta
                RequestTableStorage request = await RequestTableStorageRepository.GetRequest(track.RowKey, requestId);

                // authorized
                if (request == null || track.RowKey != request.PartitionKey) return req.CreateResponse(HttpStatusCode.Unauthorized);

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
