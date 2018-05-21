using FlashBot.Engine.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;

namespace FlashBot.Engine.Models
{
    public class TrackDTO
    {
        public string id { get; set; }
        public bool is_private { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool has_image { get; set; }
    }

    public class Track
    {
        public string id { get; set; }
        public bool is_private { get; set; }
        public string owner_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public bool has_image { get; set; }
        public int subscribers { get; set; }

        public Track(TrackAuth trackAuth)
        {
            id = trackAuth.RowKey;
            owner_id = trackAuth.PartitionKey;
            is_private = trackAuth.is_private;
            name = trackAuth.name;
            description = trackAuth.description;
            tags = string.IsNullOrEmpty(trackAuth.tags) ? new List<string>() : trackAuth.tags.Split(',').ToList();
            has_image = trackAuth.has_image;
            subscribers = trackAuth.subscribers;
        }

        public Track()
        {

        }
    }

    public class TrackAuth : TableEntity
    {
        public string name { get; set; }
        public bool is_private { get; set; }
        public string track_key { get; set; }
        public string track_secret { get; set; }
        public int rate_limit { get; set; }
        public bool rate_limit_exceeded { get; set; }
        public string description { get; set; }
        public string tags { get; set; }
        public bool has_image { get; set; }
        public int subscribers { get; set; }

        public TrackAuth(string ownerId, string trackId)
        {
            PartitionKey = ownerId;
            RowKey = trackId;
            track_key = AuthRepository.GenerateRandomString(64);
            track_secret = AuthRepository.GenerateSHA256(RowKey + track_key);
        }

        public TrackAuth()
        {
        }
    }

    public class KeySecret
    {
        public string Key { get; set; }
        public string Secret { get; set; }
    }
}
