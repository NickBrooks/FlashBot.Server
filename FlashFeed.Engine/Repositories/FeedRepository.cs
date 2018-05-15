using FlashFeed.Engine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class FeedRepository
    {
        public static async Task<List<PostQueryDTO>> GetFeed(string userId, string fromTime = null, string continuationTime = null)
        {
        }
    }
}
