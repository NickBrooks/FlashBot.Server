using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abstrack.Engine.Models;
using Abstrack.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Abstrack.Functions.Functions.API.RequestControllers
{
    public static class DeleteRequest
    {
        [FunctionName("DeleteRequest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "request/{requestId}")]HttpRequestMessage req, string requestId, TraceWriter log)
        {
            try
            {
                // check request key provided
                bool isValid = Guid.TryParse(requestId, out Guid guidResult);
                if (!isValid) return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get request key to do checks
                IEnumerable<string> authValues = req.Headers.GetValues("X-Request-Key");
                var track = await TrackRepository.GetTrackByRequestKey(authValues.FirstOrDefault());

                // authorized
                if (track == null) return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get requestmeta
                RequestMeta requestMeta = await RequestMetaRepository.GetRequestMeta(requestId);

                // authorized
                if (requestMeta == null || track.RowKey != requestMeta.PartitionKey) return req.CreateResponse(HttpStatusCode.Unauthorized);

                // delete the request
                RequestMetaRepository.DeleteRequestMeta(requestMeta);
                RequestRepository.DeleteRequest(requestMeta.RowKey);

                // return response
                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
