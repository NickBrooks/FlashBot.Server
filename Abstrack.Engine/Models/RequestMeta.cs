using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Abstrack.Engine.Models
{
    public class RequestMeta : TableEntity
    {
        public RequestMeta(string trackId, string requestId)
        {
            PartitionKey = trackId;
            RowKey = requestId;
        }

        public RequestMeta() { }

        public DateTime Date_Created { get; set; }
    }
}
