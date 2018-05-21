using FlashBot.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashBot.Engine.Repositories
{
    public class ExtendedUserRepository
    {
        public static async Task<ExtendedUser> CreateExtendedUser(ExtendedUser user)
        {
            return await TableStorageRepository.CreateExtendedUser(user);
        }

        public static async Task<ExtendedUser> GetExtendedUser(string userId)
        {
            return await TableStorageRepository.GetExtendedUser(userId);
        }

        public static async Task<ExtendedUser> UpdateExtendedUser(ExtendedUser user)
        {
            return await TableStorageRepository.UpdateExtendedUser(user);
        }

        public static async void IncrementTrackCount(string userId, bool isPrivate)
        {
            var extendedUser = await GetExtendedUser(userId);

            if (isPrivate)
                extendedUser.Private_Tracks++;
            else
                extendedUser.Public_Tracks++;

            await UpdateExtendedUser(extendedUser);
        }

        public static async void DecrementTrackCount(string userId, bool isPrivate)
        {
            var extendedUser = await GetExtendedUser(userId);

            if (isPrivate)
                extendedUser.Private_Tracks--;
            else
                extendedUser.Public_Tracks--;

            await UpdateExtendedUser(extendedUser);
        }
    }
}
