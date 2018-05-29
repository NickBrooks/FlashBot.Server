
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FlashBot.Engine;
using FlashBot.Engine.Repositories;
using FlashBot.Engine.Models;
using System;
using System.Collections.Generic;

namespace FlashBot.Functions.TrackControllers
{
    public static class FollowTrack
    {
        [FunctionName("FollowTrack")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track/{trackId}/follow")]HttpRequest req, string trackId, TraceWriter log)
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

                // trackFollow body
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                TrackFollowDTO dto = JsonConvert.DeserializeObject<TrackFollowDTO>(requestBody);

                // insert or update the follow
                TrackFollow trackFollow = new TrackFollow();
                trackFollow.feed_follow_type = dto.feed?.ToLower() == "all" || dto.feed?.ToLower() == "none" ? dto.feed.ToLower() : null;
                trackFollow.notifications_follow_type = dto.notifications?.ToLower() == "all" || dto.notifications?.ToLower() == "none" ? dto.notifications.ToLower() : null;
                trackFollow.criteria = dto.criteria;
                trackFollow.user_id = authClaim.user_id;
                trackFollow.track_id = track.RowKey;

                FollowRepository.InsertOrReplaceTrackFollow(trackFollow);

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
