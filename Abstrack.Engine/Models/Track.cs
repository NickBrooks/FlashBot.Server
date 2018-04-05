using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Abstrack.Engine.Models
{
    public class Track : TableEntity
    {
        public DateTime Date_Created { get; set; }
        public bool Is_Private { get; set; }
        public string Request_Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Rate_Limit { get; set; }
        public bool Rate_Limit_Exceeded { get; set; }
        public int Max_Requests { get; set; }

        public Track(string partitionKey)
        {
            PartitionKey = partitionKey;
            RowKey = Guid.NewGuid().ToString();
            Date_Created = DateTime.UtcNow;
            Request_Key = Tools.CreateSHA256(RowKey + Date_Created);
        }

        public Track()
        {
        }
    }
}
