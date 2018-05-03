
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FlashFeed.Functions.API.PostControllers
{
    public static class PostPost
    {
        [FunctionName("PostPost")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track/{trackId}/post")]HttpRequest req, string trackId, TraceWriter log)
        {
            try
            {
                KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(req.Headers["X-Track-Key"]);

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, keySecret.Secret))
                    return new UnauthorizedResult();

                // get post from req body
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                PostSubmitDTO post = JsonConvert.DeserializeObject<PostSubmitDTO>(requestBody);

                // validate post
                post = PostRepository.ValidatePost(post);
                if (post == null)
                    return new BadRequestResult();

                // get track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.track_key != keySecret.Key)
                    return new UnauthorizedResult();

                // check rate limit
                if (track.rate_limit_exceeded)
                    return new ForbidResult();

                // create the post
                post.track_id = trackId;
                post.track_name = track.name;
                Post newPost = await PostRepository.InsertPost(post);

                // if didn't create return bad response
                if (newPost == null)
                    return new BadRequestResult();

                return new OkObjectResult(newPost.RowKey);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
