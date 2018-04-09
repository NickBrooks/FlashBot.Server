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

namespace Abstrack.Functions.Functions.API.TrackControllers
{
    public static class GetTrack
    {
        [FunctionName("GetTrack")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                // check requestId and trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check auth if private
                if (track.is_private && !AuthRepository.ValidateSHA256(trackId, Tools.GetHeaderValue(req.Headers, "X-Track-Key"), Tools.GetHeaderValue(req.Headers, "X-Track-Secret")))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                TrackDTO trackDTO = new TrackDTO()
                {
                    id = trackId,
                    name = track.name,
                    description = track.description,
                    tags = await TrackTagRepository.GetTagsDTOByTrack(trackId)
                };

                return req.CreateResponse(HttpStatusCode.OK, trackDTO);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
