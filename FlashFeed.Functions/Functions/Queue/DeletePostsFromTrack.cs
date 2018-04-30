using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.Queue
{
    public static class DeletePostsFromTrack
    {
        [FunctionName("DeletePostsFromTrack")]
        public static async void Run([QueueTrigger("delete-posts-from-track", Connection = "AzureWebJobsStorage")]string trackId, TraceWriter log)
        {
            var listOfPostsToDelete = await PostRepository.GetListOfPostIdsInTrack(trackId);

            foreach (var post in listOfPostsToDelete)
            {
                PostRepository.DeletePost(post.id);
            }

            log.Info($"C# Queue trigger function deleted posts: {trackId}");
        }
    }
}
