using FlashFeed.Engine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class FollowRepository
    {
        public static async Task<List<TrackFollow>> GetTrackFollows(string trackId, Enums.FollowMode followMode = Enums.FollowMode.Feed)
        {
            List<TrackFollowTableEntity> results = await TableStorageRepository.GetTrackFollows(trackId, followMode);

            List<TrackFollow> trackFollows = new List<TrackFollow>();

            foreach (TrackFollowTableEntity result in results)
            {
                trackFollows.Add(TableEntityToTrackFollow(result));
            }

            return trackFollows;
        }

        public static async Task<TrackFollow> GetTrackFollow(string trackId, string userId)
        {
            TrackFollowTableEntity result = await TableStorageRepository.GetTrackFollow(trackId, userId);

            return (TableEntityToTrackFollow(result));
        }

        public static async Task<TrackFollowTableEntity> InsertOrReplaceTrackFollow(TrackFollow trackFollow)
        {
            // check user and track id
            if (trackFollow?.user_id == null || trackFollow?.track_id == null)
                return null;

            // validate criteria
            List<TagCriteria> validatedTagCriteria = new List<TagCriteria>();

            // ensure the follow is in the user follow table toowoo woo
            await InsertOrReplaceUserFollow(trackFollow.user_id, trackFollow.track_id);

            if (trackFollow.criteria != null && trackFollow.criteria.Count > 0)
            {
                foreach (var criterion in trackFollow.criteria.Take(12))
                {
                    var validatedCriterion = ValidateTagCriteria(criterion);

                    if (validatedCriterion != null)
                        validatedTagCriteria.Add(validatedCriterion);
                }
            }

            trackFollow.criteria = validatedTagCriteria;

            // get basic follow modes
            trackFollow.feed_follow_type = trackFollow.feed_follow_type != FollowType.all ? GetFollowType(trackFollow.criteria, Enums.FollowMode.Feed) : FollowType.all;
            trackFollow.notifications_follow_type = trackFollow.notifications_follow_type != FollowType.all ? GetFollowType(trackFollow.criteria, Enums.FollowMode.Notification) : FollowType.all;

            return await TableStorageRepository.InsertOrReplaceTrackFollow(TrackFollowToTableEntity(trackFollow));
        }

        public static async Task<bool> DeleteTrackFollow(string userId, string trackId, bool ownerOverride = false)
        {
            try
            {
                TrackAuth track = await TrackRepository.GetTrack(trackId);
                if (track.PartitionKey == userId && ownerOverride == false)
                    return false;

                UserFollowTableEntity userFollow = await TableStorageRepository.GetUserFollow(userId, trackId);
                TrackFollowTableEntity trackFollow = await TableStorageRepository.GetTrackFollow(trackId, userId);

                if (userFollow != null)
                    await TableStorageRepository.DeleteUserFollow(userFollow);

                if (trackFollow != null)
                    await TableStorageRepository.DeleteTrackFollow(trackFollow);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<UserFollowTableEntity> InsertOrReplaceUserFollow(string userId, string trackId)
        {
            TrackAuth track = await TrackRepository.GetTrack(trackId);

            return await TableStorageRepository.InsertOrReplaceUserFollow(new UserFollowTableEntity(userId, trackId)
            {
                description = track.description,
                has_image = track.has_image,
                is_private = track.is_private,
                name = track.name
            });
        }

        private static TagCriteria ValidateTagCriteria(TagCriteria criterion)
        {
            if (!criterion.notifications && !criterion.feed)
                return null;

            List<string> tags = new List<string>();
            foreach (var tag in criterion.tags)
            {
                if (Tools.ValidateTag(tag))
                    tags.Add(tag);
            }

            if (tags.Count == 0)
                return null;

            criterion.tags = tags;

            return criterion;
        }

        private static TrackFollow TableEntityToTrackFollow(TrackFollowTableEntity tableEntity)
        {
            return new TrackFollow()
            {
                track_id = tableEntity.PartitionKey,
                user_id = tableEntity.RowKey,
                feed_follow_type = (FollowType)Enum.Parse(typeof(FollowType), tableEntity.feed_follow_type),
                notifications_follow_type = (FollowType)Enum.Parse(typeof(FollowType), tableEntity.notifications_follow_type),
                criteria = JsonConvert.DeserializeObject<List<TagCriteria>>(tableEntity.criteria)
            };
        }

        private static TrackFollowTableEntity TrackFollowToTableEntity(TrackFollow trackFollow)
        {
            return new TrackFollowTableEntity()
            {
                PartitionKey = trackFollow.track_id,
                RowKey = trackFollow.user_id,
                feed_follow_type = trackFollow.feed_follow_type.ToString(),
                notifications_follow_type = trackFollow.notifications_follow_type.ToString(),
                criteria = JsonConvert.SerializeObject(trackFollow.criteria)
            };
        }

        private static FollowType GetFollowType(List<TagCriteria> criteria, Enums.FollowMode followMode)
        {
            foreach (var criterion in criteria)
            {
                if (followMode == Enums.FollowMode.Feed)
                {
                    if (criterion.feed)
                        return FollowType.partial;
                }
                else if (followMode == Enums.FollowMode.Notification)
                {
                    if (criterion.notifications)
                        return FollowType.partial;
                }
            }

            return FollowType.none;
        }
    }
}
