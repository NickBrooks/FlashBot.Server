using FlashFeed.Engine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class SubscriptionRepository
    {
        public static async Task<List<TrackSubscription>> GetTrackSubscriptions(string trackId, Enums.SubscriptionType subscriptionType = Enums.SubscriptionType.Feed)
        {
            return await TableStorageRepository.GetTrackSubscriptions(trackId);
        }

        public static async Task<TrackSubscription> GetTrackSubscription(string trackId, string userId)
        {
            return await TableStorageRepository.GetTrackSubscription(trackId, userId);
        }

        public static async Task<TrackSubscription> InsertTrackSubscription(TrackSubscription trackSubscription)
        {
            return await TableStorageRepository.InsertOrReplaceTrackSubscription(trackSubscription);
        }

        public static async Task<bool> DeleteTrackSubscription(TrackSubscription trackSubscription)
        {
            return await TableStorageRepository.DeleteTrackSubscription(trackSubscription);
        }
    }
}
