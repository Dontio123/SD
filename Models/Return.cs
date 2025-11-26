using System.ComponentModel.DataAnnotations;

namespace ShelfMaster.Models
{
    public class Return
    {
        [Key]
        public Guid ReturnId { get; set; } = Guid.NewGuid();
        public Guid TransactionId { get; set; }
        public DateTime DateReturned { get; set; } = DateTime.UtcNow;
        public decimal FineAmount { get; set; }
        public string? Remarks { get; set; }

        public Transaction Transaction { get; set; } = null!;
    }
}
