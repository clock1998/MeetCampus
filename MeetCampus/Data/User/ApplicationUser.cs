using Microsoft.AspNetCore.Identity;

namespace MeetCampus.Data.User;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<Guid>
{
    public UserProfile? UserProfile { get; set; }
    public AccountLifecycle? Lifecycle { get; set; }
}
