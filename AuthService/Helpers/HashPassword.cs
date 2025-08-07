using System.Security.Cryptography;
using System.Text;

namespace HotelBookingSystem.Helper
{
    public static class PasswordHasher
    {
        public static string Hash(string password) 
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}