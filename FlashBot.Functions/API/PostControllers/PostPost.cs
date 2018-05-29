
using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FlashBot.Functions.PostControllers
{
    public static class PostPost
    {
        [FunctionName("PostPost")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track/{trackId}/post")]HttpRequest req, string trackId, TraceWriter log)
        {
            try
            {
                KeySecret keySecret = AuthRepository.DecodeKeyAndSecret(req.Headers["X-Track-Key"]);
                if (keySecret == null)
                    return new UnauthorizedResult();

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId + keySecret.Key, keySecret.Secret))
                    return new UnauthorizedResult();

                // get post from req body
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                PostSubmitDTO post = JsonConvert.DeserializeObject<PostSubmitDTO>(requestBody);

                // validate post
                PostValidateDTO validatedPost = PostRepository.ValidatePost(post);
                if (validatedPost.invalid_reason != null)
                    return new BadRequestObjectResult(validatedPost.invalid_reason);

                // get track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.track_key != keySecret.Key)
                    return new UnauthorizedResult();

                // check rate limit
                if (track.rate_limit_exceeded)
                    return new ForbidResult();

                // create the post
                validatedPost.post.track_id = trackId;
                validatedPost.post.track_name = track.name;
                Post newPost = await PostRepository.InsertPost(validatedPost.post);

                // if didn't create return bad response
                if (newPost == null)
                    return new BadRequestResult();

                // convert to post DTO
                return new OkObjectResult(new PostQueryDTO()
                {
                    date_created = newPost.date_created,
                    id = newPost.RowKey,
                    summary = newPost.summary,
                    tags = newPost.tags.Split(',').ToList(),
                    title = newPost.title,
                    track_id = newPost.PartitionKey,
                    track_name = newPost.track_name,
                    type = newPost.type,
                    url = newPost.url
                });
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return new UnauthorizedResult();
            }
        }
    }
}
