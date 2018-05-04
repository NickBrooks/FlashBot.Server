using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FlashFeed.FeedFunctions
{
    public static class GetPartition
    {
        [FunctionName("GetPartition")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];
            int count = Convert.ToInt32(req.Query["count"]);

            // Create new stopwatch.
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing.
            stopwatch.Start();
            var results = await TableStorageRepository.GetAllInPartition(id, count);

            return new OkObjectResult(new { time = stopwatch.ElapsedMilliseconds, count = results.Count, data = results });
        }
    }
}
