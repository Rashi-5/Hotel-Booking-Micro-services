using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models.Auth;
using HotelBookingSystem.Services;
using HotelBookingSystem.Helper;

namespace HotelBookingSystem.Data
{
     public class DbUserStore : IUserStore
    {
        private readonly AuthDbContext _context;

        public DbUserStore(AuthDbContext context)
        {
            _context = context;
        }

        public User ValidateUser(string username, string password)
        {
             if (username == "admin" || username == "user1")
            {
            return _context.Users.FirstOrDefault(u => 
                u.Username == username && u.Password == password);
            }

            var hashedPassword = PasswordHasher.Hash(password);
            return _context.Users.FirstOrDefault(u => 
                u.Username == username && u.Password == hashedPassword);
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
        public void AddUser(User user)
        {
            user.Password = PasswordHasher.Hash(user.Password); 
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }
    }
}
