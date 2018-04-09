using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abstrack.Engine;
using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.API.TagControllers
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

                // public request so return it
                if (!track.is_private)
                {
                    List<TrackTagDTO> result = await TrackTagRepository.GetTagsDTOByTrack(trackId);
                    return req.CreateResponse(HttpStatusCode.OK, result);
                }
                // private request
                else
                {
                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, Tools.GetHeaderValue(req.Headers, "X-Track-Key"), Tools.GetHeaderValue(req.Headers, "X-Track-Secret")))
                        return req.CreateResponse(HttpStatusCode.Unauthorized);

                    List<TrackTagDTO> result = await TrackTagRepository.GetTagsDTOByTrack(trackId);
                    return req.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
