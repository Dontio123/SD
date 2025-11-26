using System.ComponentModel.DataAnnotations;

namespace ShelfMaster.Models
{
    public class Request
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public Guid BookCopyId { get; set; }
        public string RequestType { get; set; } = "Borrow"; // "Borrow" or "Return"
        public DateTime DateRequested { get; set; } = DateTime.UtcNow;
        public string RequestStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
        public BookCopy BookCopy { get; set; } = null!;
        public Transaction? Transaction { get; set; }
    }
}
