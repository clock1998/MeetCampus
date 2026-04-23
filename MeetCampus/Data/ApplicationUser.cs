using Microsoft.AspNetCore.Identity;

namespace MeetCampus.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public UserProfile? UserProfile { get; set; }
    }

}
