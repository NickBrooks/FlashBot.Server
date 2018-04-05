using System;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.Timer
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
                int requestsLastHour = RequestMetaRepository.GetRequestsLastHourAsync(track.RowKey);

                if (requestsLastHour <= track.Rate_Limit)
                {
                    track.Rate_Limit_Exceeded = false;
                    TrackRepository.UpdateTrack(track);
                    count++;
                }
            }

            log.Info($"Removed rate limit on {count} tracks.");
        }
    }
}
