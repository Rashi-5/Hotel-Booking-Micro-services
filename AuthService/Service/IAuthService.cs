using HotelBookingSystem.Models.Auth;

public interface IAuthService
{
    User ValidateUser(string username, string password);
}
