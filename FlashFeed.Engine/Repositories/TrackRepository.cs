﻿using FlashFeed.Engine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class TrackRepository
    {
        public static async Task<Track> CreateTrack(Track track)
        {
            if (track.owner_id == null || track.name == null) return null;

            var extendedUser = await ExtendedUserRepository.GetExtendedUser(track.owner_id);

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

            track.id = track.id == null ? Guid.NewGuid().ToString() : track.id;
            track.subscribers = 0;
            track.rate_limit = extendedUser.Rate_Per_Track;
            track.track_key = AuthRepository.GenerateRandomString(64);
            track.track_secret = AuthRepository.GenerateSHA256(track.id, track.track_key);

            // create the track
            var newTrack = await (dynamic)CosmosRepository<Track>.CreateItemAsync(track);
            if (newTrack == null) return null;

            // insert into table storage
            await TableStorageRepository.InsertTrackAuth(new TrackAuth(track));

            // increment user's track count
            ExtendedUserRepository.IncrementTrackCount(track.owner_id, track.is_private);

            return newTrack;
        }

        public static List<TrackAuth> GetRateLimitedTracks()
        {
            return TableStorageRepository.GetRateLimitedTracks();
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
        public static async Task<List<Track>> GetTracksByOwnerId(string ownerId)
        {
            List<Track> tracks = await (dynamic)CosmosRepository<Track>.GetItemsAsync(t => t.owner_id == ownerId);

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

        public static async void DeleteTrack(string trackId)
        {
            // decrement the count
            var track = await GetTrack(trackId);
            ExtendedUserRepository.DecrementTrackCount(track.PartitionKey, track.is_private);

            // send messages to queue
            TableStorageRepository.AddMessageToQueue("delete-posts-from-track", trackId);
            TableStorageRepository.AddMessageToQueue("delete-tracktags-from-track", trackId);

            // then delete track
            TableStorageRepository.DeleteTrack(trackId);
        }
    }
}
