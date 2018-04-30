using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.API.TagControllers
{
    public static class GetTags
    {
        [FunctionName("GetTags")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/tags")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                // check valid trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                Track track = await TrackRepository.GetTrack(trackId);
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

                List<TrackTagDTO> result = await TrackTagRepository.GetTagsDTOByTrack(trackId);
                return req.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}