using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace FlashBot.API.Queue.Posts
{
    public static class ProcessNewPostIncrementTrackTags
    {
        [FunctionName("ProcessNewPostIncrementTrackTags")]
        public static void Run([QueueTrigger("process-new-post-increment-track-tags", Connection = "TABLESTORAGE_CONNECTION")]CloudQueueMessage myQueueItem, TraceWriter log)
        {
            Post post = JsonConvert.DeserializeObject<Post>(myQueueItem.AsString);

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
