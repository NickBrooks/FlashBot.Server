using System;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.API.Timer
{
    public static class CheckRateLimitedTracks
    {
        [FunctionName("CheckRateLimitedTracks")]
        public static async void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            try
            {
                var tracks = await TrackRepository.GetRateLimitedTracks();

                int count = 0;
                foreach (var track in tracks)
                {
                    int postsLastHour = await PostRepository.PostsLastHourCount(track.RowKey);

                    if (postsLastHour <= track.rate_limit)
                    {
                        track.rate_limit_exceeded = false;
                        TrackRepository.UpdateTrack(track);
                        count++;
                    }
                }

                log.Info($"Removed rate limit on {count} tracks.");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }
    }
}
