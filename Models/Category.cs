using System.ComponentModel.DataAnnotations;

namespace ShelfMaster.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; } = Guid.NewGuid();
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
