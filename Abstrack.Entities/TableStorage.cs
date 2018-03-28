using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Abstrack.Entities
{
    public class RequestKey : TableEntity
    {
        public RequestKey(string requestKey, string trackId, string owner_id)
        {
            PartitionKey = "track_request_key";
            RowKey = requestKey;
            this.track_id = trackId;
            last_requested = DateTime.UtcNow;
        }

        public RequestKey() { }

        public string track_id { get; set; }

        public string owner_id { get; set; }

        public DateTime last_requested { get; set; }
    }

    public class TrackTag : TableEntity
    {
        public TrackTag(string trackId, string tag)
        {
            PartitionKey = trackId;
            RowKey = tag;
        }

        public TrackTag() { }

        public int count { get; set; }
    }

    // queue items
    public class TrackTagsQueueItem
    {
        public string trackId { get; set; }
        public List<string> tags { get; set; }
    }
}
