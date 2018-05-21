using Microsoft.WindowsAzure.Storage.Table;

namespace FlashBot.Engine.Models
{
    public class ExtendedUser : TableEntity
    {
        public int Public_Tracks { get; set; }
        public int Public_Tracks_Max { get; set; }
        public int Private_Tracks { get; set; }
        public int Private_Tracks_Max { get; set; }
        public int Rate_Per_Track { get; set; }
        public int Max_Track_Storage { get; set; }

        public ExtendedUser(string userId)
        {
            PartitionKey = "user";
            RowKey = userId;
            Public_Tracks = 0;
            Private_Tracks = 0;
            Public_Tracks_Max = 10;
            Private_Tracks_Max = 0;
            Rate_Per_Track = 100;
        }

        public ExtendedUser()
        {
        }
    }
}
