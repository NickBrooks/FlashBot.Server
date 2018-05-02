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

namespace FlashFeed.Functions.API.PostControllers
{
    public static class GetPosts
    {
        [FunctionName("GetPosts")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/posts")]HttpRequest req, string trackId, TraceWriter log)
        {
            try
            {
                // check valid trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return new UnauthorizedResult();

                // get query object from query params
                PostQuery query = Tools.GetQueryFromQueryParams(trackId, req.Query["tags"], req.Query["continuation"]);

                // get the track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return new UnauthorizedResult();

                // private post so check key
                if (track.is_private)
                {
                    KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(req.Headers["X-Track-Key"]);

                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                        return new UnauthorizedResult();

                    // validate track key
                    if (track == null || track.track_key != keySecret.Key)
                        return new UnauthorizedResult();
                }

                PostReturnObject posts = query.tags.Count > 0 ? await PostRepository.QueryPosts(query) : await PostRepository.GetPosts(query);
                return new OkObjectResult(posts);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
