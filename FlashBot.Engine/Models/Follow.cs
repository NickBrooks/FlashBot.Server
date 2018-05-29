using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace FlashBot.Engine.Models
{
    public class TrackFollowDTO
    {
        public string feed { get; set; }
        public string notifications { get; set; }
        public List<TagCriteria> criteria { get; set; }

        public TrackFollowDTO()
        {
            criteria = new List<TagCriteria>();
        }
    }

    public class UserFollowTableEntity : TableEntity
    {
        public string name { get; set; }
        public bool is_private { get; set; }
        public string description { get; set; }
        public bool has_image { get; set; }

        public UserFollowTableEntity(string userId, string trackId)
        {
            PartitionKey = userId;
            RowKey = trackId;
        }

        public UserFollowTableEntity() { }
    }

    public class TrackFollowTableEntity : TableEntity
    {
        public string feed_follow_type { get; set; }
        public string notifications_follow_type { get; set; }
        public string criteria { get; set; }

        public TrackFollowTableEntity(string trackId, string userId)
        {
            PartitionKey = trackId;
            RowKey = userId;
        }

        public TrackFollowTableEntity() { }
    }

    public class TrackFollow
    {
        public string track_id { get; set; }
        public string user_id { get; set; }
        public string feed_follow_type { get; set; }
        public string notifications_follow_type { get; set; }
        public List<TagCriteria> criteria { get; set; }

        public TrackFollow()
        {
            criteria = new List<TagCriteria>();
            feed_follow_type = "none";
            notifications_follow_type = "none";
        }
    }

    public class TagCriteria
    {
        public bool feed { get; set; }
        public bool notifications { get; set; }
        public List<string> tags { get; set; }
    }
}
