
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FlashFeed.Engine;
using FlashFeed.Engine.Repositories;
using FlashFeed.Engine.Models;
using System;

namespace FlashFeed.API.API.TrackControllers
{
    public static class FollowTrack
    {
        [FunctionName("FollowTrack")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/follow")]HttpRequest req, string trackId, TraceWriter log)
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
                if (track == null || (track.is_private && track.PartitionKey != authClaim.user_id))
                    return new UnauthorizedResult();

                // insert or update the follow
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                TrackFollow trackFollow = JsonConvert.DeserializeObject<TrackFollow>(requestBody);
                trackFollow.user_id = authClaim.user_id;
                trackFollow.track_id = track.RowKey;
                var result = await FollowRepository.InsertOrReplaceTrackFollow(trackFollow);

                if (result == null)
                    return new BadRequestObjectResult("Invalid follow parameters");

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
