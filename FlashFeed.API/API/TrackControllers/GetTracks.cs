
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using System.Collections.Generic;
using System;

namespace FlashFeed.API.API.TrackControllers
{
    public static class GetTracks
    {
        [FunctionName("GetTracks")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tracks")]HttpRequest req, string trackId, TraceWriter log)
        {
            try
            {
                string authToken = req.Headers["Authorization"];

                // validate authKey
                AuthClaim authClaim = AuthRepository.ValidateAuthClaim(authToken);
                if (authClaim == null)
                    return new UnauthorizedResult();

                List<TrackDTO> tracks = await FollowRepository.GetUserFollows(authClaim.user_id);

                return new OkObjectResult(new { count = tracks.Count, data = tracks });
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
