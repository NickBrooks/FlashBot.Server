using System;
using System.Collections.Generic;

namespace Abstrack.Engine.Models
{
    public class Request
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string body { get; set; }

        public Request()
        {
            date_created = DateTime.UtcNow;
        }
    }

    public class RequestDTO
    {
        public string id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
    }

    public class RequestReturnObject
    {
        public string continuationToken { get; set; }
        public int count { get; internal set; }
        public List<RequestDTO> data { get; set; }
    }
}
