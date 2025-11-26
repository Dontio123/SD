namespace ShelfMaster.Models
{
    public class Clerk
    {
        public Guid ClerkId { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {(string.IsNullOrWhiteSpace(MiddleName) ? "" : MiddleName + " ")}{LastName}";

        // Navigation
        public StaffAccount? Account { get; set; }
        public ICollection<Transaction> ManagedTransactions { get; set; } = new List<Transaction>();
    }
}
