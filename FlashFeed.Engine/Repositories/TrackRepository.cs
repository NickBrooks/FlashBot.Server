using FlashFeed.Engine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
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
                name = name,
                description = description,
                is_private = isPrivate,
                rate_limit = extendedUser.Rate_Per_Track,
                max_posts = extendedUser.Max_Track_Storage
            };

            // create the track
            var newTrack = await TableStorageRepository.InsertTrack(track);

            // increment user's track count
            ExtendedUserRepository.IncrementTrackCount(ownerId, isPrivate);

            return newTrack;
        }

        public static List<Track> GetRateLimitedTracks()
        {
            return TableStorageRepository.GetRateLimitedTracks();
        }

        public static void UpdateTrack(Track track)
        {
            TableStorageRepository.UpdateTrack(track);
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

        public static async Task<Track> GetTrackByPostKey(string postKey, bool rateLimited = true)
        {
            var track = await TableStorageRepository.GetTrackByPostKey(postKey);

            if (track == null)
                return null;

            if (track.rate_limit_exceeded && rateLimited)
                return null;

            return track;
        }

        public static async void DeleteTrack(string trackId)
        {
            // decrement the count
            var track = await GetTrack(trackId);
            ExtendedUserRepository.DecrementTrackCount(track.PartitionKey, track.is_private);

            // send messages to queue
            TableStorageRepository.AddMessageToQueue("delete-posts-from-track", trackId);
            TableStorageRepository.AddMessageToQueue("delete-tracktags-from-track", trackId);
            TableStorageRepository.AddMessageToQueue("delete-postmeta-from-track", trackId);

            // then delete track
            TableStorageRepository.DeleteTrack(trackId);
        }
    }
}
