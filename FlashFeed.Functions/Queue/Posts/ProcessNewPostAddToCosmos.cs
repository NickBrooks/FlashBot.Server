using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace FlashFeed.Functions.Queue.Posts
{
    public static class ProcessNewPostAddToCosmos
    {
        [FunctionName("ProcessNewPostAddToCosmos")]
        public static async void Run([QueueTrigger("process-new-post-add-to-cosmos", Connection = "TABLESTORAGE_CONNECTION")]CloudQueueMessage myQueueItem, TraceWriter log)
        {
            Post post = JsonConvert.DeserializeObject<Post>(myQueueItem.AsString);

            List<string> tags = post.tags.Split(',').ToList();

            PostQueryDTO postCosmos = new PostQueryDTO()
            {
                id = post.RowKey,
                track_id = post.PartitionKey,
                date_created = post.date_created,
                track_name = post.track_name,
                summary = post.summary,
                tags = tags,
                has_image = post.has_image,
                title = post.title,
                type = post.type,
                url = post.url
            };

            await PostRepository.InsertPostToCosmos(postCosmos);

            log.Info($"Added post to Cosmos: {post.RowKey}");
        }
    }
}
