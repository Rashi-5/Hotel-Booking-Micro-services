using HotelBookingSystem.Models.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<BookingFormModel>> GetAllBookingsAsync();
        Task<IEnumerable<BookingFormModel>> GetBookingsForUserAsync(string username);
        Task<BookingFormModel> GetBookingByIdAsync(Guid id);
        Task<BookingFormModel> AddBookingAsync(BookingFormModel booking);
        Task<BookingFormModel> UpdateBookingAsync(Guid id, BookingFormModel booking);
        Task<bool> DeleteBookingAsync(Guid id);
        Task<bool> BookingExistsAsync(Guid id);
        Task<bool> CheckAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int rooms);
    }
} 