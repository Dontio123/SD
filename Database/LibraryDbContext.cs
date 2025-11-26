using Microsoft.EntityFrameworkCore;
using ShelfMaster.Models;

namespace ShelfMaster.Database
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Clerk> Clerks { get; set; }
        public DbSet<StaffAccount> StaffAccounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Return> Returns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fix one-to-one: Transaction → Return
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Return)
                .WithOne(r => r.Transaction)
                .HasForeignKey<Return>(r => r.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.Copies)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.BookCopy)
                .WithMany(bc => bc.Transactions)
                .HasForeignKey(t => t.BookCopyId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.LoanTransactions)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<StaffAccount>()
                .HasOne(sa => sa.Clerk)
                .WithOne(c => c.Account)
                .HasForeignKey<StaffAccount>(sa => sa.ClerkId);

            base.OnModelCreating(modelBuilder);
        }
    }
}