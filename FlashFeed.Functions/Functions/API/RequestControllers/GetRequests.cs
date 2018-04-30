using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FlashFeed.Functions.Functions.API.RequestControllers
{
    public static class GetRequests
    {
        [FunctionName("GetRequests")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "track/{trackId}/requests")]HttpRequestMessage req, string trackId, TraceWriter log)
        {
            try
            {
                // check valid trackId provided
                if (!Tools.IsValidGuid(trackId))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check for continuation token
                var continuationToken = Tools.GetHeaderValue(req.Headers, "X-Continuation-Token");

                // get query object from query params
                RequestQuery query = Tools.GetQueryFromQueryParams(trackId, req.GetQueryNameValuePairs());

                // get the track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // public request so return it
                if (!track.is_private)
                {
                    RequestReturnObject result = await RequestRepository.GetRequests(trackId, query, continuationToken == null ? null : continuationToken);
                    return req.CreateResponse(HttpStatusCode.OK, result);
                }
                // private request
                else
                {
                    // validate authKey
                    if (!AuthRepository.ValidateSHA256(trackId, Tools.GetHeaderValue(req.Headers, "X-Track-Key"), Tools.GetHeaderValue(req.Headers, "X-Track-Secret")))
                        return req.CreateResponse(HttpStatusCode.Unauthorized);

                    RequestReturnObject result = await RequestRepository.GetRequests(trackId, query, continuationToken == null ? null : continuationToken);
                    return req.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
