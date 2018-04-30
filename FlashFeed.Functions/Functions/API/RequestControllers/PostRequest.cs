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
    public static class PostRequest
    {
        [FunctionName("PostRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track/{trackId}/request")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(Tools.GetHeaderValue(req.Headers, "X-Track-Key"));

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // validate request
                RequestDTO request = await req.Content.ReadAsAsync<RequestDTO>();
                request = RequestRepository.ValidateRequest(request);
                if (request == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                // get track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.track_key != keySecret.Key)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check rate limit
                if (track.rate_limit_exceeded)
                    return req.CreateResponse(HttpStatusCode.Forbidden);

                // create the request
                request.track_id = trackId;
                RequestDTO newRequest = await RequestRepository.InsertRequest(request);

                // if didn't create return bad response
                if (newRequest == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Location", newRequest.id);
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
