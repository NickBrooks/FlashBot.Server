using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abstrack.Engine.Models;

namespace Abstrack.Engine.Repositories
{
    public class TrackTagRepository
    {
        public static void InsertOrIncrementTrackTag(TrackTag trackTag)
        {
            TableStorageRepository.InsertOrIncrementTrackTag(trackTag);
        }

        public static async Task<List<TrackTag>> GetListOfTrackTagsInTrack(string trackId)
        {
            return await TableStorageRepository.GetTrackTags(trackId);
        }

        public static void DeleteTrackTag(TrackTag trackTag)
        {
            TableStorageRepository.DeleteTrackTag(trackTag);
        }
    }
}
