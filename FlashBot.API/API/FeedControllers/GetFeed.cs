using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;

namespace FlashBot.API.FeedControllers
{
    public static class GetFeed
    {
        [FunctionName("GetFeed")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feed")]HttpRequest req, TraceWriter log)
        {
            string authToken = req.Headers["Authorization"];

            if (authToken == null)
                return new UnauthorizedResult();

            // validate authKey
            AuthClaim authClaim = AuthRepository.ValidateAuthClaim(authToken);
            if (authClaim == null)
                return new UnauthorizedResult();

            return new OkObjectResult(authClaim.user_id);
        }
    }
}
