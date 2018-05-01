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

namespace FlashFeed.Functions.Functions.API.TrackControllers
{
    public static class GetTrack
    {
        [FunctionName("GetTrack")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                // check postId and trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                if (track.is_private)
                {
                    // private track
                    KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(Tools.GetHeaderValue(req.Headers, "X-Track-Key"));

                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                        return req.CreateResponse(HttpStatusCode.Unauthorized);

                    // validate track key
                    if (track == null || track.track_key != keySecret.Key)
                        return req.CreateResponse(HttpStatusCode.Unauthorized);
                }
                // TODO: Track DTO
                return req.CreateResponse(HttpStatusCode.OK, track);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
