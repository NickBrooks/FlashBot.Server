using System;
using System.Collections.Generic;
using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashBot.Functions.Timer
{
    public static class CheckRateLimitedTracks
    {
        [FunctionName("CheckRateLimitedTracks")]
        public static async void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            try
            {
                List<TrackAuth> tracks = await TrackRepository.GetRateLimitedTracks();

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
