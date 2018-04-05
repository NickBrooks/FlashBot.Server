using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abstrack.Engine.Models;

namespace Abstrack.Engine.Repositories
{
    public class RequestMetaRepository
    {
        public static void InsertRequestMeta(RequestMeta requestMeta)
        {
            TableStorageRepository.InsertRequestMeta(requestMeta);
        }

        public static async Task<List<RequestMeta>> GetListOfRequestMetaInTrackAsync(string trackId)
        {
            return await TableStorageRepository.GetListOfRequestMetaInTrack(trackId);
        }

        public static void DeleteRequestMeta(RequestMeta requestMeta)
        {
            TableStorageRepository.DeleteRequestMeta(requestMeta);
        }

        public static async Task<RequestMeta> GetRequestMeta(string requestId)
        {
            return await TableStorageRepository.GetRequestMeta(requestId);
        }
    }
}
