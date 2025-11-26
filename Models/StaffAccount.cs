using ShelfMaster.Models.Enums;

namespace ShelfMaster.Models
{
    public class StaffAccount
    {
        public Guid StaffAccountId { get; set; } = Guid.NewGuid();
        public Guid ClerkId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Clerk;

        // Navigation
        public Clerk Clerk { get; set; } = null!;
    }
}
