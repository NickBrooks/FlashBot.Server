using FlashFeed.Engine.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class TrackDTO
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<TrackTagDTO> tags { get; set; }
    }

    public class Track : TableEntity
    {
        public DateTime date_created { get; set; }
        public bool is_private { get; set; }
        public string track_key { get; set; }
        public string track_secret { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int rate_limit { get; set; }
        public bool rate_limit_exceeded { get; set; }
        public int max_requests { get; set; }

        public Track(string partitionKey)
        {
            PartitionKey = partitionKey;
            RowKey = Guid.NewGuid().ToString();
            date_created = DateTime.UtcNow;
            track_key = AuthRepository.GenerateRandomString(64);
            track_secret = AuthRepository.GenerateSHA256(RowKey, track_key);
        }

        public Track()
        {
        }
    }

    public class KeySecret
    {
        public string Key { get; set; }
        public string Secret { get; set; }
    }
}
