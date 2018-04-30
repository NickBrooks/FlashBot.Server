using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Linq;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class ProcessDeletePost
    {
        [FunctionName("ProcessDeletePost")]
        public static async void Run([QueueTrigger("process-delete-post", Connection = "TABLESTORAGE_CONNECTION")]string queueItem, TraceWriter log)
        {
            List<string> queueItemList = queueItem.Split('.').ToList();
            string trackId = queueItemList[0];
            string postId = queueItemList[1];

            var post = await PostRepository.GetPost(trackId, postId);

            PostRepository.DeletePostFromTableStorage(post);
            PostRepository.DeletePostFromCosmos(postId);

            // TODO: delete from feeds

            log.Info($"Deleted post: {post.RowKey}");
        }
    }
}
