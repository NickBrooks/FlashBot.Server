using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class ProcessNewPost
    {
        [FunctionName("ProcessNewPost")]
        public static async void Run([QueueTrigger("process-new-post-increment-track-tags", Connection = "TABLESTORAGE_CONNECTION")]string queueItem, TraceWriter log)
        {
            Post post = JsonConvert.DeserializeObject<Post>(queueItem);

            List<string> tags = post.tags.Split(',').ToList();

            // add tags to tracktag list
            foreach (var tag in tags)
            {
                TrackTagRepository.InsertOrIncrementTrackTag(new TrackTag(post.PartitionKey, tag));
            }

            // check rate limit
            Random rnd = new Random();
            if (rnd.Next(1, 8) == 3)
            {
                var track = await TrackRepository.GetTrack(post.PartitionKey);
                int postsLastHour = PostTableStorageRepository.GetPostsLastHourAsync(post.PartitionKey);

                if (postsLastHour > track.rate_limit)
                {
                    track.rate_limit_exceeded = true;
                    TrackRepository.UpdateTrack(track);
                }
            }

            log.Info($"Post processing completed for post: {post.RowKey}");
        }
    }
}
