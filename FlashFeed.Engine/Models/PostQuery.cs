using System.Collections.Generic;

namespace FlashBot.Engine.Models
{
    public class PostQuery
    {
        public string track_id { get; set; }
        public List<string> tags { get; set; }
        public string sql { get; set; }
        public string continuation { get; set; }
    }
}
