using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FlashFeed.Functions.Queue.Posts
{
    public static class ProcessNewPostIncrementTrackTags
    {
        [FunctionName("ProcessNewPostIncrementTrackTags")]
        public static void Run([QueueTrigger("process-new-post-increment-track-tags", Connection = "TABLESTORAGE_CONNECTION")]string myQueueItem, TraceWriter log)
        {
            Post post = JsonConvert.DeserializeObject<Post>(myQueueItem);

            string[] tags = post.tags.Split(',');

            // add tags to tracktag list
            foreach (var tag in tags)
            {
                TrackTagRepository.InsertOrIncrementTrackTag(new TrackTag(post.PartitionKey, tag));
            }

            log.Info($"Tags incremented for post: {post.RowKey}");
        }
    }
}
