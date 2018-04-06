using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abstrack.Engine;
using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.API.RequestControllers
{
    public static class GetRequests
    {
        [FunctionName("GetRequests")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "requests")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                // get query object from query params
                RequestQuery query = Tools.GetQueryFromQueryParams(req.GetQueryNameValuePairs());
                if (query == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get the track
                Track track = await TrackRepository.GetTrack(query.trackId);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // public request so return it
                if (!track.Is_Private)
                {
                    var result = await RequestRepository.GetRequests(query.sql);
                    return req.CreateResponse(HttpStatusCode.OK, result);
                }
                // private request
                else
                {
                    // get request key to do checks
                    var requestKey = Tools.GetRequestKeyFromHeaders(req.Headers);
                    if (requestKey == null)
                        return req.CreateResponse(HttpStatusCode.Unauthorized);

                    // validate using track key
                    if (requestKey.Length == 64)
                    {
                        if (track.Request_Key != requestKey)
                            return req.CreateResponse(HttpStatusCode.Unauthorized);

                        var result = await RequestRepository.GetRequests(query.sql);
                        return req.CreateResponse(HttpStatusCode.OK, result);
                    }

                    if (requestKey.Length == 128)
                    {
                        // todo user requestKey
                    }

                    return req.CreateResponse(HttpStatusCode.Unauthorized);
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
