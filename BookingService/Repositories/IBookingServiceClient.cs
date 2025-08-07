using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
    public interface IBookingServiceClient
    {
        Task<IEnumerable<BookingDetailsDto>> GetAllBookingsAsync();
        Task<IEnumerable<BookingDetailsDto>> GetBookingsForUserAsync(string username);
        Task<BookingDetailsDto> GetBookingByIdAsync(Guid id);
        Task<BookingDetailsDto> CreateBookingAsync(BookingDetailsDto booking);
        Task<BookingDetailsDto> UpdateBookingAsync(Guid id, BookingDetailsDto booking);
        Task<bool> DeleteBookingAsync(Guid id);
        Task<bool> BookingExistsAsync(Guid id);
        Task<bool> CheckAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int rooms);
    }
} 