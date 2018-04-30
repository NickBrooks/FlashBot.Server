using FlashFeed.Engine.Models;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class ContinuationTokenRepository
    {
        public static void InsertContinuationToken(ContinuationToken token)
        {
            TableStorageRepository.InsertContinuationToken(token);
        }

        public static async Task<ContinuationToken> GetContinuationToken(string trackId, string rowKey)
        {
            return await TableStorageRepository.GetContinuationToken(trackId, rowKey);
        }

        public static void DeleteContinuationToken(ContinuationToken token)
        {
            TableStorageRepository.DeleteContinuationToken(token);
        }
    }
}
