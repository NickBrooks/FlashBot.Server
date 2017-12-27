using System;
using System.Collections.Generic;

namespace Abstrack.Entities
{
    public class Request
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public Uri url { get; set; }

        public Request()
        {
            date_created = DateTime.UtcNow;
        }
    }
}
