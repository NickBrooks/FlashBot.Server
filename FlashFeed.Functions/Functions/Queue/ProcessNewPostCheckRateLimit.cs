using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class ProcessNewPostCheckRateLimit
    {
        [FunctionName("ProcessNewPostCheckRateLimit")]
        public static async void Run([QueueTrigger("process-new-post-check-rate-limit", Connection = "TABLESTORAGE_CONNECTION")]string queueItem, TraceWriter log)
        {
            var track = await TrackRepository.GetTrack(queueItem);
            int postsLastHour = PostTableStorageRepository.PostsLastHourCount(queueItem);

            if (postsLastHour > track.rate_limit)
            {
                track.rate_limit_exceeded = true;
                TrackRepository.UpdateTrack(track);
            }

            log.Info($"Rate limit checked for track: {queueItem}");
        }
    }
}
