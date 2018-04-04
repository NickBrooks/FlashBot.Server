using Abstrack.Data.Models;
using System.Threading.Tasks;

namespace Abstrack.Data.Repositories
{
    public class TrackRepository
    {
        public static async Task<Track> CreateTrack(string ownerId, string name, string description, bool isPrivate = false)
        {
            if (ownerId == null || name == null) return null;

            Track newTrack = new Track(ownerId)
            {
                Name = name,
                Description = description,
                Is_Private = isPrivate
            };

            // create the track
            return await TableStorageRepository.CreateTrack(newTrack);
        }
    }
}
