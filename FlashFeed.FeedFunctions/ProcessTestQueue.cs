using System;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.FeedFunctions
{
    public static class ProcessTestQueue
    {
        [FunctionName("ProcessTestQueue")]
        public static async void Run([QueueTrigger("process-test-queue", Connection = "TABLESTORAGE_CONNECTION")]string myQueueItem, TraceWriter log)
        {
            try
            {
                await TableStorageRepository.InsertTrackAuthTest(new TrackAuth()
                {
                    PartitionKey = myQueueItem,
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
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
