
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using FlashFeed.Engine.Repositories;
using System;
using FlashFeed.Engine.Models;

namespace FlashFeed.Functions
{
    public static class TestFunction
    {
        [FunctionName("TestFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            try
            {
                // check refresh token exists
                string authToken = req.Headers["Authorization"];
                if (authToken == null)
                    return new UnauthorizedResult();

                AuthClaim authenticated = AuthRepository.ValidateAuthClaim(authToken);
                if (authenticated == null)
                    return new UnauthorizedResult();

                return new OkObjectResult(authenticated);
            }
            catch
            {
                return new UnauthorizedResult();
            }
        }
    }
}
