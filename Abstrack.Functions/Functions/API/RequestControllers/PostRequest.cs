using Abstrack.Engine;
using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abstrack.Functions.Functions.API.RequestControllers
{
    public static class PostRequest
    {
        [FunctionName("PostRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var requestKey = Tools.GetHeaderValue(req.Headers, "X-Request-Key");
                if (requestKey == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get track
                Track track = await TrackRepository.GetTrackByRequestKey(requestKey);
                if (track == null)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check rate limit
                if (track.Rate_Limit_Exceeded)
                    return req.CreateResponse(HttpStatusCode.Forbidden);

                // create the request
                Request request = await req.Content.ReadAsAsync<Request>();
                request.track_id = track.RowKey;
                var newRequest = await RequestRepository.InsertRequest(request);

                // if didn't create return bad response
                if (newRequest == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Location", newRequest.track_id);
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
