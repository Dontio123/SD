using ShelfMaster.Models.Enums;

namespace ShelfMaster.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        // Account
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        // Role
        public UserRole Role { get; set; } = UserRole.Student;

        public string FullName => $"{FirstName} {(string.IsNullOrEmpty(MiddleName) ? "" : MiddleName + " ")}{LastName}";

        // Navigation
        public ICollection<Transaction> LoanTransactions { get; set; } = new List<Transaction>();
    }
}