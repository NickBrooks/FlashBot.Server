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

namespace FlashBot.Functions.PostControllers
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

                // get the track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return new UnauthorizedResult();

                // private track so check keys
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

                // get query object from query params
                PostQuery query = Tools.GetQueryFromQueryParams(trackId, req.Query["tags"], req.Query["continuation"]);

                PostReturnObject posts = query.tags.Count > 0 ? await PostRepository.QueryPosts(query) : await PostRepository.GetPosts(query);
                return new OkObjectResult(posts);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return new BadRequestObjectResult(e.Message);
            }
        }
    }
}
