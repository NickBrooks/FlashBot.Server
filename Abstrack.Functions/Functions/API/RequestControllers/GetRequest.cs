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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "request/{requestId}")]HttpRequestMessage req, string requestId, TraceWriter log)
        {
            try
            {
                // check request Id provided
                if (!Tools.IsValidGuid(requestId))
                    return req.CreateResponse(HttpStatusCode.NotFound);

                // get the request
                Request request = await RequestRepository.GetRequest(requestId);
                if (request == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                Track track = await TrackRepository.GetTrack(request.track_id);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // public request so return it
                if (!track.Is_Private)
                    return req.CreateResponse(HttpStatusCode.OK, request);

                // get request key to do checks
                var requestKey = Tools.GetHeaderValue(req.Headers, "X-Request-Key");
                if (requestKey == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // validate using track key
                if (requestKey.Length == 64)
                {
                    if (track.Request_Key != requestKey || track.RowKey != request.track_id)
                        return req.CreateResponse(HttpStatusCode.NotFound);

                    return req.CreateResponse(HttpStatusCode.OK, request);
                }

                if (requestKey.Length == 128)
                {
                    // todo user requestKey
                }

                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
