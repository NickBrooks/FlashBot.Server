using FlashFeed.Engine.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class Track
    {
        public string id { get; set; }
        public bool is_private { get; set; }
        public string owner_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public string has_image { get; set; }
        public int subscribers { get; set; }
    }

    public class TrackAuth : TableEntity
    {
        public bool is_private { get; set; }
        public string track_key { get; set; }
        public string track_secret { get; set; }
        public int rate_limit { get; set; }
        public bool rate_limit_exceeded { get; set; }
        public int max_posts { get; set; }

        public TrackAuth(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            track_key = AuthRepository.GenerateRandomString(64);
            track_secret = AuthRepository.GenerateSHA256(RowKey, track_key);
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
