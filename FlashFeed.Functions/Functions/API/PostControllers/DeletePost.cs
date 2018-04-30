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
    public static class DeletePost
    {
        [FunctionName("DeletePost")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "track/{trackId}/post/{postId}")]HttpRequestMessage req, string trackId, string postId, TraceWriter log)
        {
            try
            {
                KeySecret keySecret = AuthRepository.DecodeKeyAndSecretFromBase64(Tools.GetHeaderValue(req.Headers, "X-Track-Key"));

                // validate authKey
                if (!AuthRepository.ValidateSHA256(trackId, keySecret))
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // get track
                Track track = await TrackRepository.GetTrack(trackId);
                if (track == null || track.track_key != keySecret.Key)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                // delete the post
                TableStorageRepository.AddMessageToQueue("process-delete-post", $"{trackId}.{postId}");

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
