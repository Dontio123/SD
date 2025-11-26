using ShelfMaster.Models;
using ShelfMaster.Models.Enums;

namespace ShelfMaster.Database
{
    public static class DatabaseSeeder
    {
        public static void Seed(LibraryDbContext context)
        {
            var hasChanges = false;

            hasChanges |= EnsureCatalog(context);
            hasChanges |= EnsureStudentAccount(context);
            hasChanges |= EnsureStaffAccount(
                context,
                username: "librarian",
                password: "lib123",
                UserRole.Clerk,
                clerk =>
                {
                    clerk.FirstName = "Jane";
                    clerk.LastName = "Smith";
                    clerk.Email = "librarian@example.com";
                    clerk.ContactNumber = "09999999999";
                });

            hasChanges |= EnsureStaffAccount(
                context,
                username: "superadmin",
                password: "admin123",
                UserRole.Admin,
                clerk =>
                {
                    clerk.FirstName = "Alex";
                    clerk.LastName = "SuperAdmin";
                    clerk.Email = "admin@shelfmaster.io";
                    clerk.ContactNumber = "09171234567";
                });

            if (hasChanges)
            {
                context.SaveChanges();
            }
        }

        private static bool EnsureCatalog(LibraryDbContext context)
        {
            bool changed = false;

            var fiction = EnsureCategory(context, "Fiction", "General fiction titles", ref changed);
            var programming = EnsureCategory(context, "Programming", "Software development resources", ref changed);

            changed |= EnsureBookWithCopy(
                context,
                programming,
                "The Pragmatic Programmer",
                "Andrew Hunt & David Thomas",
                "978-0201616224",
                "Addison-Wesley",
                "A-1");

            changed |= EnsureBookWithCopy(
                context,
                fiction,
                "To Kill a Mockingbird",
                "Harper Lee",
                "978-0446310789",
                "Harper Perennial",
                "B-2");

            return changed;
        }

        private static Category EnsureCategory(LibraryDbContext context, string name, string description, ref bool changedFlag)
        {
            var category = context.Categories.FirstOrDefault(c => c.CategoryName == name);
            if (category != null) return category;

            category = new Category
            {
                CategoryName = name,
                Description = description
            };
            context.Categories.Add(category);
            changedFlag = true;
            return category;
        }

        private static bool EnsureBookWithCopy(
            LibraryDbContext context,
            Category category,
            string title,
            string author,
            string isbn,
            string publisher,
            string shelf)
        {
            bool changed = false;
            var book = context.Books.FirstOrDefault(b => b.Title == title);

            if (book == null)
            {
                book = new Book
                {
                    Title = title,
                    Author = author,
                    ISBN = isbn,
                    Publisher = publisher,
                    Category = category
                };
                context.Books.Add(book);
                changed = true;
            }

            if (!context.BookCopies.Any(bc => bc.BookId == book.BookId))
            {
                context.BookCopies.Add(new BookCopy
                {
                    Book = book,
                    CopyNumber = 1,
                    ShelfLocation = shelf
                });
                changed = true;
            }

            return changed;
        }

        private static bool EnsureStudentAccount(LibraryDbContext context)
        {
            if (context.Users.Any(u => u.Username == "student"))
            {
                return false;
            }

            var student = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "student",
                PasswordHash = "student123",
                Email = "student@example.com",
                Role = UserRole.Student,
                College = "Engineering",
                Department = "Computer Science",
                ContactNumber = "09123456789",
                Address = "Sample Address"
            };

            var account = new UserAccount
            {
                User = student,
                Username = student.Username,
                Email = student.Email,
                Password = "student123"
            };

            context.Users.Add(student);
            context.UserAccounts.Add(account);

            return true;
        }

        private static bool EnsureStaffAccount(
            LibraryDbContext context,
            string username,
            string password,
            UserRole role,
            Action<Clerk> configureClerk)
        {
            if (context.StaffAccounts.Any(sa => sa.Username == username))
            {
                return false;
            }

            var clerk = new Clerk();
            configureClerk(clerk);

            var account = new StaffAccount
            {
                Clerk = clerk,
                Username = username,
                Password = password,
                Role = role
            };

            clerk.Account = account;

            context.Clerks.Add(clerk);
            context.StaffAccounts.Add(account);
            return true;
        }
    }
}