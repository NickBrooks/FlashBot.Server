using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FlashBot.Engine.Models
{
    public class ContinuationToken : TableEntity
    {
        public string Continuation_Token { get; set; }

        public ContinuationToken(string trackId, string rowKey)
        {
            PartitionKey = trackId;
            RowKey = rowKey;
        }

        public ContinuationToken()
        {
        }
    }
}
