using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashBot.Functions.TrackControllers
{
    public static class GetTracks
    {
        [FunctionName("GetTracks")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tracks")]HttpRequest req, TraceWriter log)
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
