using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Data;
using HotelBookingSystem.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace HotelBookingSystem.Repositories
{
    public class DatabaseBookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public DatabaseBookingRepository(BookingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingFormModel>> GetAllBookingsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Bookings.ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<BookingFormModel>> GetBookingsForUserAsync(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Bookings.Where(b => b.Username == username).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<BookingFormModel> GetBookingByIdAsync(Guid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<BookingFormModel> AddBookingAsync(BookingFormModel booking)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return booking;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<BookingFormModel> UpdateBookingAsync(Guid id, BookingFormModel booking)
        {
            await _semaphore.WaitAsync();
            try
            {
                var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
                if (existingBooking != null)
                {
                    existingBooking.CustomerName = booking.CustomerName;
                    existingBooking.CheckIn = booking.CheckIn;
                    existingBooking.CheckOut = booking.CheckOut;
                    existingBooking.Username = booking.Username;
                    existingBooking.Note = booking.Note;
                    existingBooking.NumberOfRooms = booking.NumberOfRooms;
                    existingBooking.TotalPrice = booking.TotalPrice;
                    existingBooking.RoomType = booking.RoomType;
                    existingBooking.Adult = booking.Adult;
                    existingBooking.Children = booking.Children;
                    existingBooking.BookingType = booking.BookingType;
                    existingBooking.Frequency = booking.Frequency;
                    existingBooking.Interval = booking.Interval;
                    existingBooking.Days = booking.Days;
                    existingBooking.Amenities = booking.Amenities;
                    await _context.SaveChangesAsync();
                    return existingBooking;
                }
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteBookingAsync(Guid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> BookingExistsAsync(Guid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Bookings.AnyAsync(b => b.BookingId == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> CheckAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int rooms)
        {
            await _semaphore.WaitAsync();
            try
            {
                var allRooms = RoomDataHelper.GetDefaultRooms();
                var selectedRoom = allRooms.FirstOrDefault(r => r.RoomName == roomType);
                if (selectedRoom == null)
                    return false;

                for (var date = checkIn; date <= checkOut; date = date.AddDays(1))
                {
                    int booked = await _context.Bookings
                        .Where(b => b.RoomType == roomType && b.CheckIn <= date && b.CheckOut >= date)
                        .SumAsync(b => b.NumberOfRooms);
                    if (booked + rooms > selectedRoom.NumberOfRooms)
                        return false;
                }
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<(bool Success, string Message, BookingFormModel Booking)> CreateBookingWithAvailabilityCheckAsync(BookingFormModel booking, List<DateTime> bookingDates)
        {
            await _semaphore.WaitAsync();
            try
            {
                var allRooms = RoomDataHelper.GetDefaultRooms();
                var selectedRoom = allRooms.FirstOrDefault(r => r.RoomName == booking.RoomType);
                if (selectedRoom == null)
                    return (false, $"Room type '{booking.RoomType}' not found.", null);

                // Check availability for all booking dates in a single transaction
                foreach (var date in bookingDates)
                {
                    int booked = await _context.Bookings
                        .Where(b => b.RoomType == booking.RoomType && b.CheckIn <= date && b.CheckOut >= date)
                        .SumAsync(b => b.NumberOfRooms);
                    if (booked + booking.NumberOfRooms > selectedRoom.NumberOfRooms)
                        return (false, $"Not enough rooms available for '{booking.RoomType}' on {date:yyyy-MM-dd}.", null);
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return (true, "Booking created successfully.", booking);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
} 