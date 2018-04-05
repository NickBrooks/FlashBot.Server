using Abstrack.Engine.Models;

namespace Abstrack.Engine.Repositories
{
    public class TrackTagRepository
    {
        public static void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            TableStorageRepository.InsertOrIncrementTrackTag(trackTag);
        }
    }
}
