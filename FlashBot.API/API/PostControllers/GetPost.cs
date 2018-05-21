using FlashBot.Engine;
using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FlashBot.API.PostControllers
{
    public static class GetPost
    {
        [FunctionName("GetPost")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/post/{postId}")]HttpRequest req, string trackId, string postId, TraceWriter log)
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
                        if (!AuthRepository.ValidateSHA256(trackId + keySecret.Key, keySecret.Secret))
                            return new UnauthorizedResult();

                        // validate track key
                        if (track.track_key != keySecret.Key)
                            return new UnauthorizedResult();
                    }
                    else
                        return new UnauthorizedResult();
                }

                // get the post
                Post post = await PostRepository.GetPost(trackId, postId);
                if (post == null)
                    return new UnauthorizedResult();

                // convert to post DTO
                return new OkObjectResult(new PostDTO()
                {
                    body = post.body,
                    date_created = post.date_created,
                    id = post.RowKey,
                    summary = post.summary,
                    tags = post.tags.Split(',').ToList(),
                    title = post.title,
                    track_id = post.PartitionKey,
                    track_name = post.track_name,
                    type = post.type,
                    url = post.url
                });
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
