using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class PostQuery
    {
        public string track_id { get; set; }
        public List<string> tags { get; set; }
        public string sql { get; set; }
        public string continuation_time { get; set; }
    }

    public class CosmosQueryPagingResults<T> where T : class
    {
        public List<T> results { get; set; }
        public string continuationToken { get; set; }
    }
}
