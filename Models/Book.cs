namespace ShelfMaster.Models
{
    public class Book
    {
        public Guid BookId { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int? CopyrightYear { get; set; }
        public string? Summary { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    }
}