using System;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Queue.Posts
{
    public static class ProcessNewPostCheckRateLimit
    {
        [FunctionName("ProcessNewPostCheckRateLimit")]
        public static async void Run([QueueTrigger("process-new-post-check-rate-limit", Connection = "TABLESTORAGE_CONNECTION")]string myQueueItem, TraceWriter log)
        {
            var track = await TrackRepository.GetTrack(myQueueItem);
            int postsLastHour = await PostRepository.PostsLastHourCount(myQueueItem);

            if (postsLastHour > track.rate_limit)
            {
                track.rate_limit_exceeded = true;
                TrackRepository.UpdateTrack(track);
            }

            log.Info($"Rate limit checked for track: {myQueueItem}");
        }
    }
}
