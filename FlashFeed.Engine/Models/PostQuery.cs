using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class PostQuery
    {
        public List<string> tags { get; set; }
        public string sql { get; set; }
    }

    public class CosmosQueryPagingResults<T> where T : class
    {
        public List<T> results { get; set; }

        public string continuationToken { get; set; }
    }
}
