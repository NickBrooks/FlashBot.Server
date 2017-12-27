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
        public byte[] request_key { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public Track()
        {
            id = new Guid().ToString();
            date_created = DateTime.UtcNow;

            using (SHA512 shaM = new SHA512Managed())
            {
                request_key = shaM.ComputeHash(Encoding.UTF8.GetBytes(id + date_created.ToString()));
            }
        }
    }
}
