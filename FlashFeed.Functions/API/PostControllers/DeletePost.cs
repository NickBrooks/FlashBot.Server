using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace FlashFeed.Functions.API.PostControllers
{
    public static class DeletePost
    {
        [FunctionName("DeletePost")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "track/{trackId}/post/{postId}")]HttpRequest req, string trackId, string postId, TraceWriter log)
        {
            try
            {
                KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(req.Headers["X-Track-Key"]);

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, keySecret.Secret))
                    return new UnauthorizedResult();

                // get track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.track_key != keySecret.Key)
                    return new UnauthorizedResult();

                // get post
                Post post = await PostRepository.GetPost(trackId, postId);
                if (post == null || track.RowKey != post.PartitionKey)
                    return new UnauthorizedResult();

                // delete the post
                TableStorageRepository.AddMessageToQueue("process-delete-post", $"{trackId}.{postId}");

                // return response
                return new OkResult();
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
