using MeetCampus.Data.User;

namespace MeetCampus.Data.Profiles
{
    public sealed class UserProfileImage
    {
        public Guid Id { get; set; }
        public string ImageKey { get; set; } = null!;
        public bool IsAvatar { get; set; } 
        public Guid UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = null!;
    }
}
