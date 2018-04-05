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

            var extendedUser = await ExtendedUserRepository.GetExtendedUser(ownerId);

            // check private maxed out
            if (isPrivate)
            {
                if (extendedUser.Private_Tracks >= extendedUser.Private_Tracks_Max)
                    return null;
            }

            // check public maxed out
            if (!isPrivate)
            {
                if (extendedUser.Public_Tracks >= extendedUser.Public_Tracks_Max)
                    return null;
            }

            Track track = new Track(ownerId)
            {
                Name = name,
                Description = description,
                Is_Private = isPrivate,
                Rate_Limit = extendedUser.Rate_Per_Track,
                Max_Requests = extendedUser.Max_Track_Storage
            };

            // create the track
            var newTrack = await TableStorageRepository.CreateTrack(track);

            // increment user's track count
            ExtendedUserRepository.IncrementTrackCount(ownerId, isPrivate);

            return newTrack;
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

        public static async void DeleteTrack(string trackId)
        {
            // decrement the count
            var track = await GetTrack(trackId);
            ExtendedUserRepository.DecrementTrackCount(track.PartitionKey, track.Is_Private);

            // send a message to track 
            TableStorageRepository.AddMessageToQueue("delete-requests-from-track", trackId);

            // then delete track
            TableStorageRepository.DeleteTrack(trackId);
        }
    }
}
