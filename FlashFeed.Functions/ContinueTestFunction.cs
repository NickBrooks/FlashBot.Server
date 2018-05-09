using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;

namespace FlashFeed.Functions
{
    public static class ContinueTestFunction
    {
        [FunctionName("ContinueTestFunction")]
        public static async void Run([QueueTrigger("continue-test-function", Connection = "TABLESTORAGE_CONNECTION")]string myQueueItem, TraceWriter log)
        {
            try
            {
                int total = Convert.ToInt32(Tools.Base64Decode(myQueueItem));
                if (total >= 10000)
                    return;

                int count = 100;

                await TableStorageRepository.AddMessageToQueueAsync("continue-test-function", Tools.Base64Encode((total + count).ToString()));

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        await TableStorageRepository.InsertTrackAuthTest(new TrackAuth()
                        {
                            PartitionKey = Guid.NewGuid().ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            has_image = true,
                            description = "This is the long description I was talking about.",
                            is_private = false,
                            rate_limit = 10,
                            rate_limit_exceeded = false,
                            tags = "chicken,haha,lol",
                            subscribers = 17,
                            track_key = Guid.NewGuid().ToString(),
                            track_secret = Guid.NewGuid().ToString(),
                            name = "This is the title"
                        });
                    }
                    catch
                    {
                        await TableStorageRepository.AddMessageToQueueAsync("process-test-queue", Guid.NewGuid().ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
