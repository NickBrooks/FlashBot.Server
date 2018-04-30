using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace FlashFeed.Functions.Functions.Queue.Posts
{
    public static class ProcessNewPostAddImage
    {
        [FunctionName("ProcessNewPostAddImage")]
        public static void Run([QueueTrigger("process-new-post-add-image", Connection = "TABLESTORAGE_CONNECTION")]string queueItem, TraceWriter log)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData("https://fbcdn-sphotos-h-a.akamaihd.net/hphotos-ak-xpf1/v/t34.0-12/10555140_10201501435212873_1318258071_n.jpg?oh=97ebc03895b7acee9aebbde7d6b002bf&oe=53C9ABB0&__gda__=1405685729_110e04e71d9");
                var result = webClient.ResponseHeaders["Content-Type"];

                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (var yourImage = Image.FromStream(mem))
                    {
                        // If you want it as Png
                        yourImage.Save("path_to_your_file.png", ImageFormat.Png);

                        // If you want it as Jpeg
                        yourImage.Save("path_to_your_file.jpg", ImageFormat.Jpeg);
                    }
                }

            }

            log.Info($"C# Queue trigger function processed: {queueItem}");
        }
    }
}
