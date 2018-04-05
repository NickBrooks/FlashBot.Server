using System;
using System.Collections.Generic;

namespace Abstrack.Engine.Models
{
    public class Request
    {
        public string Id { get; set; }
        public string Track_Id { get; set; }
        public DateTime Date_Created { get; set; }
        public List<string> Tags { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }

        public Request()
        {
            Date_Created = DateTime.UtcNow;
        }
    }
}
