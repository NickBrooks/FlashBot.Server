using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FlashFeed.Functions.API.PostControllers
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

                // private post so check key
                if (track.is_private)
                {
                    KeySecret keySecret = AuthRepository.DecodeKeyAndSecret(req.Headers["X-Track-Key"]);

                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, keySecret.Secret))
                        return new UnauthorizedResult();

                    // validate track key
                    if (track == null || track.track_key != keySecret.Key)
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
