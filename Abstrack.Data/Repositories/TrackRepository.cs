using Abstrack.Data.Entities;
using System.Threading.Tasks;

namespace Abstrack.Data.Repositories
{
    public class TrackRepository
    {
        public static async Task<Track> CreateTrack(string ownerId, string name, string description)
        {
            if (ownerId == null || name == null) return null;

            Track newTrack = new Track()
            {
                owner_id = ownerId,
                name = name,
                description = description
            };

            // create the track
            Track createdTrack = await (dynamic)CosmosRepository<Track>.CreateItemAsync(newTrack);

            // add request key
            RequestKey requestKey = new RequestKey(createdTrack.id, createdTrack.request_key);
            TableStorageRepository.AddRequestKey(requestKey);

            return createdTrack;
        }
    }
}
