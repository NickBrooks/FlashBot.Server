using System;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.Timer
{
    public static class CheckRateLimitedTracks
    {
        [FunctionName("CheckRateLimitedTracks")]
        public static void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var tracks = TrackRepository.GetRateLimitedTracks();

            int count = 0;
            foreach (var track in tracks)
            {
                int postsLastHour = PostRepository.PostsLastHourCount(track.RowKey);

                if (postsLastHour <= track.rate_limit)
                {
                    track.rate_limit_exceeded = false;
                    TrackRepository.UpdateTrack(track);
                    count++;
                }
            }

            log.Info($"Removed rate limit on {count} tracks.");
        }
    }
}
