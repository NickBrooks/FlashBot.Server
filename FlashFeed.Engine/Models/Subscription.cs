using Microsoft.WindowsAzure.Storage.Table;

namespace FlashFeed.Engine.Models
{
    public class TrackSubscription : TableEntity
    {
        public bool feed { get; set; }
        public bool notification { get; set; }
        public string feed_criteria { get; set; }
        public string notification_criteria { get; set; }

        public TrackSubscription(string trackId, string userId)
        {
            PartitionKey = trackId;
            RowKey = userId;
        }

        public TrackSubscription() { }
    }
}
