using ShelfMaster.Models.Enums;

namespace ShelfMaster.Models
{
    public class Transaction
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid BookCopyId { get; set; }
        public Guid? RequestId { get; set; }
        public Guid? ClerkId { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public DateTime DateRequested { get; set; } = DateTime.UtcNow;
        public DateTime? DateIssued { get; set; }
        public DateTime? DueDate { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public BookCopy BookCopy { get; set; } = null!;
        public Request? Request { get; set; }
        public Return? Return { get; set; }
        public Clerk? Clerk { get; set; }
    }
}
