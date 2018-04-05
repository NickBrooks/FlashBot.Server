﻿using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace Abstrack.Engine.Models
{
    public class TrackTag : TableEntity
    {
        public TrackTag(string trackId, string tag)
        {
            PartitionKey = trackId;
            RowKey = tag;
        }

        public TrackTag() { }

        public int Count { get; set; }
    }

    // queue items
    public class TrackTagsQueueItem
    {
        public string Track_Id { get; set; }
        public List<string> Tags { get; set; }
    }
}