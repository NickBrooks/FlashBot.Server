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
    public static class GetPosts
    {
        [FunctionName("GetPosts")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/posts")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                // check valid trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get query object from query params
                PostQuery query = Tools.GetQueryFromQueryParams(trackId, req.GetQueryNameValuePairs());

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

                PostReturnObject posts = query.tags.Count > 0 ? await PostRepository.QueryPosts(query) : PostRepository.GetPosts(query);
                return req.CreateResponse(HttpStatusCode.OK, posts);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
