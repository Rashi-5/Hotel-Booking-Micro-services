using HotelBookingSystem.Models.Auth;

namespace HotelBookingSystem.Services
{
    public interface IUserStore
    {
        User ValidateUser(string username, string password);
        User GetUserByUsername(string username);
        IEnumerable<User> GetAllUsers();
        void AddUser(User user);
        bool UserExists(string username);
    }
}