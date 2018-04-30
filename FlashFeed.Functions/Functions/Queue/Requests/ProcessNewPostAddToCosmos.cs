using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class ProcessNewPostAddToCosmos
    {
        [FunctionName("ProcessNewPostAddToCosmos")]
        public static async void Run([QueueTrigger("process-new-post-add-to-cosmos", Connection = "TABLESTORAGE_CONNECTION")]string queueItem, TraceWriter log)
        {
            Post post = JsonConvert.DeserializeObject<Post>(queueItem);

            List<string> tags = post.tags.Split(',').ToList();

            PostCosmos postCosmos = new PostCosmos()
            {
                id = post.RowKey,
                track_id = post.PartitionKey,
                date_created = post.date_created,
                summary = post.summary,
                tags = tags,
                title = post.title,
                type = post.type,
                url = post.url
            };

            await PostRepository.InsertPostToCosmos(postCosmos);

            log.Info($"Added post to Cosmos: {post.RowKey}");
        }
    }
}
