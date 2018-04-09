using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Abstrack.Engine.Models
{
    public class Track : TableEntity
    {
        public DateTime date_created { get; set; }
        public bool is_private { get; set; }
        public string request_key { get; set; }
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
            request_key = Tools.CreateSHA256(RowKey + date_created);
        }

        public Track()
        {
        }
    }
}
