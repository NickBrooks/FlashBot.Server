using Abstrack.Engine.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Abstrack.Engine.Models
{
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
            track_key = AuthRepository.GenerateRandomString(32);
            track_secret = AuthRepository.GenerateSHA256(RowKey, track_key);
        }

        public Track()
        {
        }
    }
}
