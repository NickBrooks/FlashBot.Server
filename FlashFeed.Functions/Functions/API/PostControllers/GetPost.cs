using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlashFeed.Functions.Functions.API.PostControllers
{
    public static class GetPost
    {
        [FunctionName("GetPost")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/post/{postId}")]HttpRequestMessage req, string trackId, string postId, TraceWriter log)
        {
            try
            {
                // check postId and trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // private post so check key
                if (track.is_private)
                {
                    KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(Tools.GetHeaderValue(req.Headers, "X-Track-Key"));

                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                        return req.CreateResponse(HttpStatusCode.Unauthorized);

                    // validate track key
                    if (track == null || track.track_key != keySecret.Key)
                        return req.CreateResponse(HttpStatusCode.Unauthorized);
                }

                // get the post
                Post post = await PostRepository.GetPost(trackId, postId);
                if (post == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // convert to post DTO
                return req.CreateResponse(HttpStatusCode.OK, new PostDTO()
                {
                    body = post.body,
                    date_created = post.date_created,
                    id = post.RowKey,
                    summary = post.summary,
                    tags = post.tags.Split(',').ToList(),
                    title = post.title,
                    track_id = post.PartitionKey,
                    track_name = post.track_name,
                    type = post.type,
                    url = post.url
                });
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
