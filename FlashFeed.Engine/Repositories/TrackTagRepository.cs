using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlashFeed.Engine.Models;

namespace FlashFeed.Engine.Repositories
{
    public class TrackTagRepository
    {
        public static void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            TableStorageRepository.InsertOrIncrementTrackTag(trackTag);
        }

        public static async Task<List<TrackTag>> GetTagsByTrack(string trackId)
        {
            return await TableStorageRepository.GetTagsByTrack(trackId);
        }

        public static void DeleteTrackTag(TrackTag trackTag)
        {
            TableStorageRepository.DeleteTrackTag(trackTag);
        }

        public static async Task<List<TrackTagDTO>> GetTagsDTOByTrack(string trackId)
        {
            var trackTags = await GetTagsByTrack(trackId);

            List<TrackTagDTO> trackDTO = new List<TrackTagDTO>();

            foreach (var trackTag in trackTags)
            {
                trackDTO.Add(new TrackTagDTO()
                {
                    tag = trackTag.RowKey,
                    count = trackTag.count
                });
            }

            return trackDTO;
        }
    }
}
