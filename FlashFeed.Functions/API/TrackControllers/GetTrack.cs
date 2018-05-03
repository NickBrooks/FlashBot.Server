using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace FlashFeed.Functions.API.TrackControllers
{
    public static class GetTrack
    {
        [FunctionName("GetTrack")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}")]HttpRequest req, string trackId, TraceWriter log)
        {
            try
            {
                // check postId and trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return new UnauthorizedResult();

                // get the track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return new UnauthorizedResult();

                if (track.is_private)
                {
                    // private track
                    KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(req.Headers["X-Track-Key"]);

                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                        return new UnauthorizedResult();

                    // validate track key
                    if (track == null || track.track_key != keySecret.Key)
                        return new UnauthorizedResult();
                }
                // TODO: Track DTO
                return new OkObjectResult(track);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
