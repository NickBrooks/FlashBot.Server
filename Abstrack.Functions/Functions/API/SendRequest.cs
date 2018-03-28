using Abstrack.Entities;
using Abstrack.Functions.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abstrack.Functions.Functions.API
{
    public static class SendRequest
    {
        [FunctionName("SendRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                // get request key to do checks
                IEnumerable<string> authValues = req.Headers.GetValues("X-Request-Key");
                var requestKey = await TableStorageRepository.GetRequestKey(authValues.FirstOrDefault());

                // authorized
                if (requestKey == null) return req.CreateResponse(HttpStatusCode.Unauthorized);

                // check rate limit
                if (DateTime.UtcNow < requestKey.last_requested.AddMilliseconds(1000)) return req.CreateResponse(HttpStatusCode.Forbidden);

                // create the request
                Request request = await req.Content.ReadAsAsync<Request>();
                request.track_id = requestKey.track_id;
                var newRequest = await RequestRepository.CreateAsync(request);

                // if didn't create return bad response
                if (newRequest == null) return req.CreateResponse(HttpStatusCode.BadRequest);

                // update the requestKey
                requestKey.last_requested = DateTime.UtcNow;
                TableStorageRepository.UpdateRequestKey(requestKey);

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
