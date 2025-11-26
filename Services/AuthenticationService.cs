using Microsoft.EntityFrameworkCore;
using ShelfMaster.Database;
using ShelfMaster.Models;
using ShelfMaster.Models.Enums;

namespace ShelfMaster.Services
{
    public class AuthenticationService
    {
        private readonly LibraryDbContext _context;

        // Constructor injection — THIS IS THE FIX!
        public AuthenticationService(LibraryDbContext context)
        {
            _context = context;
        }

        public class LoginResult
        {
            public User? Student { get; set; }
            public Clerk? Staff { get; set; }
            public UserRole? Role { get; set; }
        }

        public LoginResult? Login(string usernameOrEmail, string password)
        {
            // Staff Login
            var staff = _context.StaffAccounts
                .Include(s => s.Clerk)
                .FirstOrDefault(s => s.Username == usernameOrEmail && s.Password == password);

            if (staff != null)
                return new LoginResult { Staff = staff.Clerk, Role = staff.Role };

            // Student Login
            var student = _context.UserAccounts
                .Include(u => u.User)
                .FirstOrDefault(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail)
                                  && u.Password == password);

            if (student != null)
                return new LoginResult { Student = student.User, Role = student.User.Role };

            return null;
        }

        // Optional: Get staff role
        public UserRole? GetStaffRole(string username)
        {
            return _context.StaffAccounts
                .FirstOrDefault(s => s.Username == username)?.Role;
        }
    }
}