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
    public static class PostRequest
    {
        [FunctionName("PostRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track/{trackId}/request")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                // validate authKey
                var trackKey = Tools.GetHeaderValue(req.Headers, "X-Track-Key");
                var trackSecret = Tools.GetHeaderValue(req.Headers, "X-Track-Secret");
                if (!AuthRepository.ValidateSHA256(trackId, trackKey, trackSecret))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check rate limit
                if (track.rate_limit_exceeded)
                    return req.CreateResponse(HttpStatusCode.Forbidden);

                // create the request
                RequestDTO request = await req.Content.ReadAsAsync<RequestDTO>();
                request.track_id = track.RowKey;
                var newRequest = await RequestRepository.InsertRequest(request);

                // if didn't create return bad response
                if (newRequest == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Location", newRequest.track_id);
                return response;
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
