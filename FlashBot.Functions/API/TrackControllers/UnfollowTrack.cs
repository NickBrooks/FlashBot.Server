using FlashBot.Engine;
using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace FlashBot.Functions.TrackControllers
{
    public static class UnfollowTrack
    {
        [FunctionName("UnfollowTrack")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "track/{trackId}/follow")]HttpRequest req, string trackId, TraceWriter log)
        {
            try
            {
                // check postId and trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return new UnauthorizedResult();

                string authToken = req.Headers["Authorization"];

                if (authToken == null)
                    return new UnauthorizedResult();

                // validate authKey
                AuthClaim authClaim = AuthRepository.ValidateAuthClaim(authToken);
                if (authClaim == null)
                    return new UnauthorizedResult();

                // get the track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.is_private)
                    return new UnauthorizedResult();

                // insert or update the follow
                var result = await FollowRepository.DeleteTrackFollow(authClaim.user_id, track.RowKey);

                if (!result)
                    return new BadRequestResult();

                return new OkResult();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
