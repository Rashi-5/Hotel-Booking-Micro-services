using ChatbotService.Models;

namespace ChatbotService.Services
{
    public interface IBookingServiceClient
    {
        Task<IEnumerable<BookingFormModel>> GetAllBookingsAsync();
        Task<IEnumerable<BookingFormModel>> GetBookingsForUserAsync(string username);
        Task<bool> CheckRoomAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int numberOfRooms);
        Task<IEnumerable<string>> GetRoomAmenitiesAsync(string roomType);
    }
}
