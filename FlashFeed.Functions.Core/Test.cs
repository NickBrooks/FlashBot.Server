
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlashFeed.Functions.Core
{
    public static class TestFunction
    {
        [FunctionName("Test")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            string url = req.Query["url"];

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url);
            Stream inputStream = await response.Content.ReadAsStreamAsync();

            return url != null
                ? (ActionResult)new OkObjectResult($"Hello, {url}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
