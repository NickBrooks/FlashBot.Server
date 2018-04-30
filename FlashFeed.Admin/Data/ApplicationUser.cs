using Microsoft.AspNetCore.Identity;

namespace FlashFeed.Admin.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public Subscription Subscription { get; set; }
    }

    public enum Subscription
    {
        Free
    }
}
