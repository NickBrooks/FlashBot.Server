using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Abstrack.Engine.Models
{
    public class RequestDTO
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string body { get; set; }
    }

    public class RequestQueryDTO
    {
        public string id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
    }

    public class RequestTableStorage : TableEntity
    {
        public string tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string body { get; set; }
        public long _ts { get; set; }

        public RequestTableStorage(string id, string trackId, DateTime timestamp)
        {
            PartitionKey = trackId;
            RowKey = id;
            Timestamp = timestamp;
        }

        public RequestTableStorage()
        {
        }
    }

    public class RequestCosmos
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
    }

    public class RequestReturnObject
    {
        public string continuation_token { get; set; }
        public int count { get; internal set; }
        public List<RequestQueryDTO> data { get; set; }
    }
}
