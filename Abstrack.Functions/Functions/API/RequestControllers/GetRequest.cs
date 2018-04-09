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
    public static class GetRequest
    {
        [FunctionName("GetRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/request/{requestId}")]HttpRequestMessage req, string trackId, string requestId, TraceWriter log)
        {
            try
            {
                // check requestId and trackId provided
                if (!Tools.IsValidGuid(requestId) || !Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the request
                RequestDTO request = await RequestRepository.GetRequest(trackId, requestId);
                if (request == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // public request so return it
                if (!track.is_private)
                    return req.CreateResponse(HttpStatusCode.OK, request);

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, Tools.GetHeaderValue(req.Headers, "X-Track-Key"), Tools.GetHeaderValue(req.Headers, "X-Track-Secret")))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                return req.CreateResponse(HttpStatusCode.OK, request);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
