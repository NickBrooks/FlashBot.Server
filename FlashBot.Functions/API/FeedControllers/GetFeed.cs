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

namespace FlashBot.Functions.FeedControllers
{
    public static class GetFeed
    {
        [FunctionName("GetFeed")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feed")]HttpRequest req, TraceWriter log)
        {
            try
            {
                string authToken = req.Headers["Authorization"];

                if (authToken == null)
                    return new UnauthorizedResult();

                // validate authKey
                AuthClaim authClaim = AuthRepository.ValidateAuthClaim(authToken);
                if (authClaim == null)
                    return new UnauthorizedResult();

                // get optional params
                GetFeedDTO feedDTO = new GetFeedDTO()
                {
                    from = Convert.ToInt64(req.Query["from"]),
                    continuation = Convert.ToInt64(req.Query["continuation"])
                };

                List<PostQueryDTO> feed = await FeedRepository.GetFeed(authClaim.user_id, feedDTO.from, feedDTO.continuation);

                return new OkObjectResult(new ReturnFeedDTO()
                {
                    count = feed.Count,
                    from = feed.Count > 0 ? feed[0].date_created : 0,
                    continuation = feed.Count >= 30 ? feed[feed.Count - 1].date_created : 0,
                    data = feed
                });
            }
            catch (Exception e)
            {
                return new BadRequestResult();
            }
        }

        class GetFeedDTO
        {
            public long from { get; set; }
            public long continuation { get; set; }
        }

        class ReturnFeedDTO
        {
            public int count { get; set; }
            public long from { get; set; }
            public long continuation { get; set; }
            public List<PostQueryDTO> data { get; set; }
        }
    }
}
