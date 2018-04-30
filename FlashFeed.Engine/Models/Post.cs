﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace FlashFeed.Engine.Models
{
    public class PostDTO
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string body { get; set; }
        public string url { get; set; }
    }

    public class PostQueryDTO
    {
        public string id { get; set; }
        public DateTime date_created { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
    }

    public class Post : TableEntity
    {
        public DateTime date_created { get; set; }
        public string type { get; set; }
        public string tags { get; set; }
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

    public class PostCosmos
    {
        public string id { get; set; }
        public string track_id { get; set; }
        public DateTime date_created { get; set; }
        public string type { get; set; }
        public List<string> tags { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string url { get; set; }
    }

    public class PostReturnObject
    {
        public string continuation_token { get; set; }
        public int count { get; internal set; }
        public List<PostQueryDTO> data { get; set; }
    }
}