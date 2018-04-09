using System.Collections.Generic;

namespace Abstrack.Engine.Models
{
    public class RequestQuery
    {
        public string trackId { get; set; }
        public List<string> tags { get; set; }
        public string sql { get; set; }
    }

    public class CosmosQueryPagingResults<T> where T : class
    {
        public List<T> results { get; set; }

        public string continuationToken { get; set; }
    }
}
