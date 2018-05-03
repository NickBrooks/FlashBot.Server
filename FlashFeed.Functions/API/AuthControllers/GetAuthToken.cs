using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace FlashFeed.Functions.API.AuthControllers
{
    public static class GetAuthToken
    {
        [FunctionName("GetAuthToken")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "auth")]HttpRequest req, TraceWriter log)
        {
            try
            {
                // check refresh token exists
                string refreshToken = req.Headers["Authorization"];
                if (refreshToken == null)
                    return new UnauthorizedResult();

                // validate and destroy it
                string userId = await AuthRepository.GetRefreshTokenUserAndDestroyToken(refreshToken);
                if (userId == null)
                    return new UnauthorizedResult();

                // create JWTObject
                JWTObject jwtObject = await AuthRepository.GenerateJWTObject(userId);

                return new OkObjectResult(jwtObject);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }
    }
}
