using Abstrack.Engine.Models;
using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abstrack.Engine.Repositories
{
    public class RequestRepository
    {
        public static async Task<Request> InsertRequest(Request request)
        {
            if (request == null) return null;
            if (request.body == null) return null;
            if (request.tags.Count > 12) return null;

            request.title = request.title.Length > 80 ? request.title.Substring(0, 80) : request.title;
            request.body = request.body.Length > 5000 ? request.body.Substring(0, 5000) : request.body;
            request.summary = GenerateSummary(request.body);
            request.tags = Tools.ValidateTags(request.tags);
            request.date_created = DateTime.UtcNow;

            var result = await (dynamic)CosmosRepository<Request>.CreateItemAsync(request);
            request.id = result.id;

            // add to queue for further processing
            TableStorageRepository.AddMessageToQueue("process-new-request", JsonConvert.SerializeObject(request));

            return result;
        }

        public static async Task<Request> GetRequest(string requestId)
        {
            return await CosmosRepository<Request>.GetItemByIdAsync(requestId);
        }

        public static async Task<RequestReturnObject> GetRequests(RequestQuery query, string continuationToken = null)
        {
            ContinuationToken token = null;

            if (continuationToken != null)
                token = await ContinuationTokenRepository.GetContinuationToken(query.trackId, continuationToken);

            var result = await CosmosRepository<RequestDTO>.GetItemsSqlWithPagingAsync(query.sql, 30, token?.Continuation_Token == null ? null : token.Continuation_Token);

            // generate continuationToken
            string newToken = null;
            if (!string.IsNullOrEmpty(result.continuationToken))
            {
                newToken = Tools.CreateSHA256(result.continuationToken + DateTime.UtcNow.ToString());
                ContinuationTokenRepository.InsertContinuationToken(new ContinuationToken(query.trackId, newToken)
                {
                    Continuation_Token = result.continuationToken,
                });
            }

            if (token != null)
                ContinuationTokenRepository.DeleteContinuationToken(token);

            return new RequestReturnObject()
            {
                continuation_token = newToken,
                count = result.results.Count(),
                data = result.results
            };
        }

        public static async Task<List<Request>> GetListOfRequestIdsInTrack(string trackId)
        {
            return await CosmosRepository<Request>.GetItemsSqlAsync($"SELECT r.id FROM r WHERE r.track_id = '{trackId}'");
        }

        public static async void DeleteRequest(string requestId)
        {
            await CosmosRepository<Request>.DeleteItemAsync(requestId);
        }

        private static string GenerateSummary(string body)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var plainText = Markdown.ToPlainText(body, pipeline);

            return plainText.Length > 140 ? plainText.Substring(0, 140) : plainText;
        }
    }
}
