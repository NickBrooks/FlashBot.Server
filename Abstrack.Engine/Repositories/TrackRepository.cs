using Abstrack.Engine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abstrack.Engine.Repositories
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

        /// <summary>
        /// Get tracks by ownerId.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns>List of tracks.</returns>
        public static async Task<List<Track>> GetTracks(string ownerId)
        {
            return await TableStorageRepository.GetTracks(ownerId);
        }

        public static async Task<Track> GetTrack(string trackId)
        {
            var track = await TableStorageRepository.GetTrack(trackId);

            if (track == null)
                return null;

            return track;
        }

        public static async Task<Track> GetVerifiedTrack(string trackId, string userId)
        {
            var track = await TableStorageRepository.GetTrack(trackId);

            if (track == null)
                return null;

            if (track.PartitionKey != userId)
                return null;

            return track;
        }

        public static async Task<Track> GetTrackByRequestKey(string requestKey)
        {
            var track = await TableStorageRepository.GetTrackByRequestKey(requestKey);

            if (track == null)
                return null;

            if (track.Rate_Limit_Exceeded)
                return null;

            return track;
        }
    }
}
