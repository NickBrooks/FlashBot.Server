using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlashFeed.Engine.Models;

namespace FlashFeed.Engine.Repositories
{
    public class RequestTableStorageRepository
    {
        public static async Task<RequestTableStorage> InsertRequest(RequestTableStorage request)
        {
            return await TableStorageRepository.InsertRequest(request);
        }

        public static async Task<List<RequestTableStorage>> GetRequestsInTrack(string trackId)
        {
            return await TableStorageRepository.GetRequestsInTrack(trackId);
        }

        public static void DeleteRequest(RequestTableStorage requestMeta)
        {
            TableStorageRepository.DeleteRequest(requestMeta);
        }

        public static async Task<RequestTableStorage> GetRequest(string trackId, string requestId)
        {
            return await TableStorageRepository.GetRequest(trackId, requestId);
        }

        public static int GetRequestsLastHourAsync(string track_id)
        {
            return TableStorageRepository.QueryEntities<RequestTableStorage>(t => t.PartitionKey == track_id && t.Timestamp > DateTime.UtcNow.AddHours(-1), "requests").Count; ;
        }
    }
}
