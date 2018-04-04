using Microsoft.AspNetCore.Identity;

namespace Abstrack.Admin.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public int Public_Tracks { get; set; }
        public int Private_Tracks { get; set; }
        public int Rate_Per_Track { get; set; }
        public int Max_Track_Storage { get; set; }
        public Subscription Subscription { get; set; }
    }

    public enum Subscription
    {
        Free
    }
}
