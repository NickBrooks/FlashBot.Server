using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class PostSubmitDTO : PostDTO
    {
        public string image_url { get; set; }
    }

    public class PostCosmos : PostQueryDTO
    {
        public bool is_root_post { get; set; }
        public List<string> subscriber_list { get; set; }
    }

    public class PostDTO : PostQueryDTO
    {
        public string body { get; set; }
    }

    public class PostQueryDTO
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public string track_name { get; set; }
        public long date_created { get; set; }
        public List<string> tags { get; set; }
        public string type { get; set; }
        public bool has_image { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string url { get; set; }
    }

    public class Post : TableEntity
    {
        public long date_created { get; set; }
        public string track_name { get; set; }
        public string tags { get; set; }
        public string type { get; set; }
        public bool has_image { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string body { get; set; }
        public string url { get; set; }

        public Post(string id, string trackId)
        {
            PartitionKey = trackId;
            RowKey = id;
        }

        public Post()
        {
        }
    }

    public class PostReturnObject
    {
        public string continuation { get; set; }
        public int count { get; internal set; }
        public List<PostQueryDTO> data { get; set; }
    }
}
