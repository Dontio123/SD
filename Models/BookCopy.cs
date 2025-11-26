using ShelfMaster.Models.Enums;

namespace ShelfMaster.Models
{
    public class BookCopy
    {
        public Guid BookCopyId { get; set; } = Guid.NewGuid();
        public Guid BookId { get; set; }
        public int CopyNumber { get; set; }
        public string ShelfLocation { get; set; } = string.Empty;
        public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

        // Navigation
        public Book Book { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

