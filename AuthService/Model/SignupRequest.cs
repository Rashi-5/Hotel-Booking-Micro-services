using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models.Auth
{
    public class SignupRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [MinLength(6, ErrorMessage = "Password should be minimum 6 characters")]
        public string Password { get; set; }
    }
}