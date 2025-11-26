namespace ShelfMaster.Models
{
    public class UserAccount
    {
        public Guid UserAccountId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Navigation
        public User User { get; set; } = null!;
    }
}
