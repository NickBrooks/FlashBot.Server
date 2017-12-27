using Abstrack.Data.Engine;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Abstrack.Data.Entities
{
    public class Track
    {
        public string id { get; set; }
        public string owner_id { get; set; }
        public DateTime date_created { get; set; }
        public string request_key { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public Track()
        {
            id = Guid.NewGuid().ToString();
            date_created = DateTime.UtcNow;
            request_key = Tools.CreateSHA256(id + date_created);
        }
    }
}
