using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;

namespace FlashFeed.API.Queue.Posts
{
    public static class ProcessNewPostCheckRateLimit
    {
        [FunctionName("ProcessNewPostCheckRateLimit")]
        public static async void Run([QueueTrigger("process-new-post-check-rate-limit", Connection = "TABLESTORAGE_CONNECTION")]CloudQueueMessage myQueueItem, TraceWriter log)
        {
            var track = await TrackRepository.GetTrack(myQueueItem.AsString);
            int postsLastHour = await PostRepository.PostsLastHourCount(myQueueItem.AsString);

            if (postsLastHour > track.rate_limit)
            {
                track.rate_limit_exceeded = true;
                TrackRepository.UpdateTrack(track);
            }

            log.Info($"Rate limit checked for track: {myQueueItem.AsString}");
        }
    }
}
