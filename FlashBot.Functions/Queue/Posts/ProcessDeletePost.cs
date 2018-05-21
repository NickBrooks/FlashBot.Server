using FlashBot.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;

namespace FlashBot.API.Queue.Posts
{
    public static class ProcessDeletePost
    {
        [FunctionName("ProcessDeletePost")]
        public static async void Run([QueueTrigger("process-delete-post", Connection = "QUEUESTORAGE_CONNECTION")]CloudQueueMessage myQueueItem, TraceWriter log)
        {
            string[] queueItemList = myQueueItem.AsString.Split('.');
            string trackId = queueItemList[0];
            string postId = queueItemList[1];

            var post = await PostRepository.GetPost(trackId, postId);

            PostRepository.DeletePostFromTableStorage(post);
            PostRepository.DeletePostFromCosmos(postId);

            if (post.has_image)
            {
                PostRepository.DeleteImages(postId);
            }

            // TODO: delete from feeds
            log.Info($"Deleted post: {post.RowKey}");
        }
    }
}
