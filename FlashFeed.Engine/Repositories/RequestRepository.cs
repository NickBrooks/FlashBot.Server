using FlashFeed.Engine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class RequestRepository
    {
        public static async Task<RequestDTO> InsertRequest(RequestDTO requestDTO)
        {
            // cosmos insertion
            RequestCosmos requestCosmos = new RequestCosmos()
            {
                date_created = requestDTO.date_created,
                summary = requestDTO.summary,
                tags = requestDTO.tags,
                title = requestDTO.title,
                track_id = requestDTO.track_id
            };

            var cosmosResult = await (dynamic)CosmosRepository<RequestCosmos>.CreateItemAsync(requestCosmos);

            if (cosmosResult == null)
                return null;

            // table storage insertion
            RequestTableStorage requestTableStorage = new RequestTableStorage(cosmosResult.id, requestDTO.track_id)
            {
                summary = requestDTO.summary,
                tags = string.Join(",", requestDTO.tags),
                title = requestDTO.title,
                body = requestDTO.body,
                date_created = requestDTO.date_created,
                _ts = cosmosResult._ts
            };

            await RequestTableStorageRepository.InsertRequest(requestTableStorage);

            // add to queue for further processing
            TableStorageRepository.AddMessageToQueue("process-new-request", JsonConvert.SerializeObject(requestTableStorage));

            return cosmosResult;
        }

        public static async Task<RequestDTO> GetRequest(string trackId, string requestId)
        {
            var result = await RequestTableStorageRepository.GetRequest(trackId, requestId);

            if (result == null)
                return null;

            return new RequestDTO()
            {
                id = requestId,
                track_id = trackId,
                date_created = result.date_created,
                title = result.title,
                body = result.body,
                summary = result.summary,
                tags = result.tags.Split(',').ToList()
            };
        }

        public static async Task<RequestReturnObject> GetRequests(string trackId, RequestQuery query, string continuationToken = null)
        {
            ContinuationToken token = null;

            if (continuationToken != null)
                token = await ContinuationTokenRepository.GetContinuationToken(trackId, continuationToken);

            var result = await CosmosRepository<RequestQueryDTO>.GetItemsSqlWithPagingAsync(query.sql, 50, token?.Continuation_Token == null ? null : token.Continuation_Token);

            // generate continuationToken
            string newToken = null;
            if (!string.IsNullOrEmpty(result.continuationToken))
            {
                newToken = AuthRepository.GenerateSHA256(result.continuationToken, DateTime.UtcNow.ToString());
                ContinuationTokenRepository.InsertContinuationToken(new ContinuationToken(trackId, newToken)
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

        public static async Task<List<RequestDTO>> GetListOfRequestIdsInTrack(string trackId)
        {
            return await CosmosRepository<RequestDTO>.GetItemsSqlAsync($"SELECT r.id FROM r WHERE r.track_id = '{trackId}'");
        }

        public static async void DeleteRequest(string requestId)
        {
            await CosmosRepository<RequestDTO>.DeleteItemAsync(requestId);
        }

        public static RequestDTO ValidateRequest(RequestDTO requestDTO)
        {
            if (requestDTO == null) return null;
            if (requestDTO.body == null) return null;
            if (requestDTO.tags.Count > 12) return null;

            // check for provided summary
            if (!string.IsNullOrEmpty(requestDTO.summary))
                requestDTO.summary = requestDTO.summary.Length > 140 ? requestDTO.summary.Substring(0, 140) : requestDTO.summary;

            requestDTO.title = requestDTO.title.Length > 80 ? requestDTO.title.Substring(0, 80) : requestDTO.title;
            requestDTO.body = requestDTO.body.Length > 10000 ? requestDTO.body.Substring(0, 10000) : requestDTO.body;
            requestDTO.summary = string.IsNullOrEmpty(requestDTO.summary) ? Tools.GenerateSummary(requestDTO.body) : requestDTO.summary;
            requestDTO.tags = Tools.ValidateTags(requestDTO.tags);
            requestDTO.date_created = DateTime.UtcNow;

            return requestDTO;
        }
    }
}
