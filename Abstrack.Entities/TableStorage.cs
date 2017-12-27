using Microsoft.WindowsAzure.Storage.Table;

namespace Abstrack.Entities
{
    public class RequestKey : TableEntity
    {
        public RequestKey(string trackId, string requestKey)
        {
            PartitionKey = "track_request_key";
            RowKey = requestKey;
            this.track_id = trackId;
        }

        public RequestKey() { }

        public string track_id { get; set; }
    }
}
