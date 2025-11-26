using Microsoft.EntityFrameworkCore;
using ShelfMaster.Database;
using ShelfMaster.Models;
using ShelfMaster.Models.Enums;

namespace ShelfMaster.Services
{
    public record DashboardStats(int TotalStudents, int TotalClerks, int TotalBooks, int TotalCopies, int PendingRequests, int ActiveLoans);

    public class LibraryService
    {
        private readonly IDbContextFactory<LibraryDbContext> _dbFactory;

        public LibraryService(IDbContextFactory<LibraryDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Transaction>> GetPendingRequestsAsync()
        {
            await using var context = _dbFactory.CreateDbContext();
            return await BuildTransactionQuery(context)
                .Where(t => t.Status == TransactionStatus.Pending)
                .OrderBy(t => t.DateRequested)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetActiveLoansAsync()
        {
            await using var context = _dbFactory.CreateDbContext();
            return await BuildTransactionQuery(context)
                .Where(t => t.Status == TransactionStatus.Issued || (t.Status == TransactionStatus.Approved && t.Return == null))
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetUserActiveLoansAsync(Guid userId)
        {
            await using var context = _dbFactory.CreateDbContext();
            return await BuildTransactionQuery(context)
                .Where(t => t.UserId == userId && t.Status == TransactionStatus.Issued && t.Return == null)
                .OrderByDescending(t => t.DateIssued ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetAvailableBooksAsync()
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.BookCopies
                .Include(bc => bc.Book)
                    .ThenInclude(b => b.Category)
                .Where(bc => bc.Status == BookCopyStatus.Available)
                .OrderBy(bc => bc.Book.Title)
                .ToListAsync();
        }

        public async Task RequestBookAsync(Guid userId, Guid bookCopyId)
        {
            await using var context = _dbFactory.CreateDbContext();

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                       ?? throw new InvalidOperationException("User not found.");

            var copy = await context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId)
                ?? throw new InvalidOperationException("Book copy not found.");

            if (copy.Status != BookCopyStatus.Available)
            {
                throw new InvalidOperationException("Book copy is no longer available.");
            }

            var request = new Request
            {
                UserId = userId,
                BookId = copy.BookId,
                BookCopyId = copy.BookCopyId,
                Book = copy.Book,
                User = user,
                BookCopy = copy
            };

            var transaction = new Transaction
            {
                UserId = userId,
                BookCopyId = copy.BookCopyId,
                Status = TransactionStatus.Pending,
                DateRequested = DateTime.UtcNow,
                Request = request,
                RequestId = request.RequestId
            };

            request.Transaction = transaction;

            context.Requests.Add(request);
            context.Transactions.Add(transaction);

            await context.SaveChangesAsync();
        }

        public async Task IssueBookAsync(Guid transactionId, Guid clerkId)
        {
            await using var context = _dbFactory.CreateDbContext();

            var transaction = await BuildTransactionQuery(context)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId)
                ?? throw new InvalidOperationException("Transaction not found.");

            if (transaction.Status != TransactionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending requests can be issued.");
            }

            if (transaction.BookCopy.Status != BookCopyStatus.Available)
            {
                throw new InvalidOperationException("Selected book copy is no longer available.");
            }

            transaction.Status = TransactionStatus.Issued;
            transaction.ClerkId = clerkId;
            transaction.DateIssued = DateTime.UtcNow;
            transaction.DueDate ??= transaction.DateIssued.Value.AddDays(7);
            transaction.BookCopy.Status = BookCopyStatus.Borrowed;

            await context.SaveChangesAsync();
        }

        public async Task ReturnBookAsync(Guid transactionId, Guid? requestedByUserId = null, string? remarks = null)
        {
            await using var context = _dbFactory.CreateDbContext();

            var transaction = await BuildTransactionQuery(context)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId)
                ?? throw new InvalidOperationException("Borrowing record not found.");

            if (transaction.Status != TransactionStatus.Issued)
            {
                throw new InvalidOperationException("This book is not currently issued or has already been returned.");
            }

            if (requestedByUserId.HasValue && transaction.UserId != requestedByUserId.Value)
            {
                throw new InvalidOperationException("This book was not borrowed under your account.");
            }

            if (transaction.Return != null)
            {
                throw new InvalidOperationException("This book was already returned.");
            }

            transaction.Status = TransactionStatus.Returned;
            transaction.BookCopy.Status = BookCopyStatus.Available;

            var returnRecord = new Return
            {
                TransactionId = transaction.TransactionId,
                DateReturned = DateTime.UtcNow,
                Remarks = remarks
            };

            transaction.Return = returnRecord;
            context.Returns.Add(returnRecord);

            await context.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            await using var context = _dbFactory.CreateDbContext();
            context.Transactions.Update(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            await using var context = _dbFactory.CreateDbContext();

            var totalStudents = await context.Users.CountAsync(u => u.Role == UserRole.Student);
            var totalClerks = await context.Clerks.CountAsync();
            var totalBooks = await context.Books.CountAsync();
            var totalCopies = await context.BookCopies.CountAsync();
            var pendingRequests = await context.Transactions.CountAsync(t => t.Status == TransactionStatus.Pending);
            var activeLoans = await context.Transactions.CountAsync(t => t.Status == TransactionStatus.Issued || (t.Status == TransactionStatus.Approved && t.Return == null));

            return new DashboardStats(totalStudents, totalClerks, totalBooks, totalCopies, pendingRequests, activeLoans);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task AddCategoryAsync(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Category name is required.");
            }

            await using var context = _dbFactory.CreateDbContext();

            var exists = await context.Categories.AnyAsync(c => c.CategoryName == name.Trim());
            if (exists)
            {
                throw new InvalidOperationException("Category already exists.");
            }

            context.Categories.Add(new Category
            {
                CategoryName = name.Trim(),
                Description = description?.Trim()
            });

            await context.SaveChangesAsync();
        }

        public async Task<Clerk> CreateStaffAccountAsync(
            string firstName,
            string? middleName,
            string lastName,
            string email,
            string contactNumber,
            string username,
            string password,
            UserRole role)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                throw new InvalidOperationException("First name and last name are required.");
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Username and password are required.");
            }

            await using var context = _dbFactory.CreateDbContext();

            var existingUser = await context.StaffAccounts.AnyAsync(sa => sa.Username == username);
            if (existingUser)
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            var clerk = new Clerk
            {
                FirstName = firstName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim(),
                ContactNumber = contactNumber.Trim()
            };

            var staffAccount = new StaffAccount
            {
                Clerk = clerk,
                Username = username.Trim(),
                Password = password,
                Role = role
            };

            clerk.Account = staffAccount;
            context.Clerks.Add(clerk);
            context.StaffAccounts.Add(staffAccount);

            await context.SaveChangesAsync();
            return clerk;
        }

        public async Task<List<Clerk>> GetClerksAsync()
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.Clerks
                .Include(c => c.Account)
                .OrderBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<User> CreateStudentAccountAsync(
            string firstName,
            string? middleName,
            string lastName,
            string email,
            string contactNumber,
            string address,
            string college,
            string department,
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                throw new InvalidOperationException("First name and last name are required.");
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Username and password are required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Email is required.");
            }

            await using var context = _dbFactory.CreateDbContext();

            var existingUser = await context.UserAccounts.AnyAsync(ua => ua.Username == username || ua.Email == email);
            if (existingUser)
            {
                throw new InvalidOperationException("Username or email is already taken.");
            }

            var user = new User
            {
                FirstName = firstName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim(),
                ContactNumber = contactNumber.Trim(),
                Address = address.Trim(),
                College = college.Trim(),
                Department = department.Trim(),
                Username = username.Trim(),
                PasswordHash = password, // In production, this should be hashed
                Role = UserRole.Student,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            var userAccount = new UserAccount
            {
                User = user,
                Username = username.Trim(),
                Email = email.Trim(),
                Password = password // In production, this should be hashed
            };

            context.Users.Add(user);
            context.UserAccounts.Add(userAccount);

            await context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetStudentsAsync()
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.Users
                .Where(u => u.Role == UserRole.Student)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        private IQueryable<Transaction> BuildTransactionQuery(LibraryDbContext context)
        {
            return context.Transactions
                .Include(t => t.User)
                .Include(t => t.BookCopy)
                    .ThenInclude(bc => bc.Book)
                        .ThenInclude(b => b.Category)
                .Include(t => t.Clerk)
                .Include(t => t.Return);
        }
    }
}