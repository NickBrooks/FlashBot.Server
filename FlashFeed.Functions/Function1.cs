
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net;
using FlashFeed.Engine;
using System;
using FlashFeed.Engine.Repositories;
using System.Threading.Tasks;

namespace FlashFeed.Functions
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string url = req.Query["uel"];

            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(url);
                string contentType = webClient.ResponseHeaders["Content-Type"];

                // generate small thumb
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        Images.CropSquare(32, input, output);

                        using (output)
                        {
                            var result = await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), Guid.NewGuid().ToString() + "/thumb_mini", contentType);

                            return new OkObjectResult(result);
                        }
                    }
                }
            }
        }
    }
}
