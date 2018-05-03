using FlashFeed.Engine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class TrackRepository
    {
        public static async Task<TrackAuth> CreateTrack(TrackAuth track)
        {
            if (track.PartitionKey == null || track.name == null) return null;

            var extendedUser = await ExtendedUserRepository.GetExtendedUser(track.PartitionKey);

            // check private maxed out
            if (track.is_private)
            {
                if (extendedUser.Private_Tracks >= extendedUser.Private_Tracks_Max)
                    return null;
            }

            // check public maxed out
            if (!track.is_private)
            {
                if (extendedUser.Public_Tracks >= extendedUser.Public_Tracks_Max)
                    return null;
            }

            track.RowKey = track.RowKey ?? Guid.NewGuid().ToString();
            track.subscribers = 0;
            track.rate_limit = extendedUser.Rate_Per_Track;
            track.track_key = AuthRepository.GenerateRandomString(64);
            track.track_secret = AuthRepository.GenerateSHA256(track.RowKey, track.track_key);

            // insert into table storage
            var newTrack = await TableStorageRepository.InsertTrackAuth(track);
            if (newTrack == null) return null;

            // insert into Cosmos
            await (dynamic)CosmosRepository<Track>.CreateItemAsync(new Track(track));

            // increment user's track count
            ExtendedUserRepository.IncrementTrackCount(track.PartitionKey, track.is_private);

            return newTrack;
        }

        public static async Task<List<TrackAuth>> GetRateLimitedTracks()
        {
            return await TableStorageRepository.GetRateLimitedTracks();
        }

        public static void UpdateTrack(TrackAuth track)
        {
            TableStorageRepository.UpdateTrack(track);
        }

        /// <summary>
        /// Get tracks by ownerId.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns>List of tracks.</returns>
        public static async Task<List<TrackAuth>> GetTracksByOwnerId(string ownerId)
        {
            List<TrackAuth> tracks = await TableStorageRepository.GetTracksByOwnerId(ownerId);

            return tracks;
        }

        public static async Task<TrackAuth> GetTrack(string trackId)
        {
            var track = await TableStorageRepository.GetTrack(trackId);

            if (track == null)
                return null;

            return track;
        }

        public static async Task<TrackAuth> GetTrackVerifyOwner(string trackId, string userId)
        {
            var track = await TableStorageRepository.GetTrack(trackId);

            if (track == null)
                return null;

            if (track.PartitionKey != userId)
                return null;

            return track;
        }

        public static async Task<bool> DeleteTrack(string trackId)
        {
            // decrement the count
            var track = await GetTrack(trackId);
            ExtendedUserRepository.DecrementTrackCount(track.PartitionKey, track.is_private);

            // send messages to queue
            TableStorageRepository.AddMessageToQueue("delete-posts-from-track", trackId);
            TableStorageRepository.AddMessageToQueue("delete-tracktags-from-track", trackId);

            // then delete track cosmos
            CosmosRepository<Track>.DeleteItemAsync(trackId);

            // then delete profile pics
            DeleteImages(trackId);

            // then delete from table storage, and return
            return await TableStorageRepository.DeleteTrack(trackId);
        }

        public static void DeleteImages(string trackId)
        {
            BlobRepository.DeleteFolder(trackId, BlobRepository.TracksContainer);
        }
    }
}
