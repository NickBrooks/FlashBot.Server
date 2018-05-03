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

                // is private
                if (track.is_private)
                {
                    string trackKeyHeader = req.Headers["X-Track-Key"];
                    string authToken = req.Headers["Authorization"];

                    if (authToken != null)
                    {
                        // validate authKey
                        AuthClaim authClaim = AuthRepository.ValidateAuthClaim(authToken);
                        if (authClaim == null)
                            return new UnauthorizedResult();

                        // check track userID matches authClaim userId
                        if (track.PartitionKey != authClaim.user_id)
                            return new UnauthorizedResult();
                    }
                    else if (trackKeyHeader != null)
                    {
                        KeySecret keySecret = AuthRepository.DecodeKeyAndSecret(trackKeyHeader);

                        // validate authKey
                        if (!AuthRepository.ValidateSHA256(trackId, keySecret.Secret))
                            return new UnauthorizedResult();

                        // validate track key
                        if (track.track_key != keySecret.Key)
                            return new UnauthorizedResult();
                    }
                    else
                        return new UnauthorizedResult();
                }

                return new OkObjectResult(new Track(track));
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
