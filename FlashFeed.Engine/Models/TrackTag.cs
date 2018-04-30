using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class TrackTagDTO
    {
        public string tag { get; set; }
        public int count { get; set; }
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
        public string Track_Id { get; set; }
        public List<string> Tags { get; set; }
    }
}
