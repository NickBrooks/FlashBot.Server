using Abstrack.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstrack.Engine.Repositories
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
    }
}
