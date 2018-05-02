
using FlashFeed.Engine;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FlashFeed.Functions
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {

                using (WebClient webClient = new WebClient())
                {
                    string url = req.Query["url"];

                    if (url == null)
                        return new BadRequestObjectResult("No Url specified.");

                    byte[] data = webClient.DownloadData(url);
                    string contentType = webClient.ResponseHeaders["Content-Type"];

                    List<string> list = new List<string>();
                    var id = Guid.NewGuid().ToString();

                    // generate small thumb
                    using (MemoryStream input = new MemoryStream(data))
                    {
                        using (MemoryStream output = new MemoryStream())
                        {
                            Images.CropSquare(150, input, output);

                            using (output)
                            {
                                var result = await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), id + "/cropped", contentType);
                                list.Add(result);
                            }
                        }
                    }

                    using (MemoryStream input = new MemoryStream(data))
                    {
                        using (MemoryStream output = new MemoryStream())
                        {
                            Images.ResizeToMax(1024, input, output);

                            using (output)
                            {
                                var result = await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), id + "/resized", contentType);
                                list.Add(result);
                            }
                        }
                    }

                    using (MemoryStream input = new MemoryStream(data))
                    {
                        using (MemoryStream output = new MemoryStream())
                        {
                            Images.GenerateHero(400, input, output);

                            using (output)
                            {
                                var result = await BlobRepository.UploadFileAsync(BlobRepository.PostsContainer, output.ToArray(), id + "/hero", contentType);
                                list.Add(result);
                            }
                        }
                    }
                    return new OkObjectResult(list);
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }
    }
}
