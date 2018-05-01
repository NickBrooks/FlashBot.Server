using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlashFeed.Functions.Functions.API.PostControllers
{
    public static class PostPost
    {
        [FunctionName("PostPost")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track/{trackId}/post")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(Tools.GetHeaderValue(req.Headers, "X-Track-Key"));

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // validate post
                PostSubmitDTO post = await req.Content.ReadAsAsync<PostSubmitDTO>();
                post = PostRepository.ValidatePost(post);
                if (post == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                // get track
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.track_key != keySecret.Key)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check rate limit
                if (track.rate_limit_exceeded)
                    return req.CreateResponse(HttpStatusCode.Forbidden);

                // create the post
                post.track_id = trackId;
                post.track_name = track.name;
                Post newPost = await PostRepository.InsertPostToTableStorage(post);

                // if didn't create return bad response
                if (newPost == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Location", newPost.RowKey);
                return response;
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
