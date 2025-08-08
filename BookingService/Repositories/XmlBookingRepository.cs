using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using System.Xml;

namespace HotelBookingSystem.Repositories
{
    public class XmlBookingRepository : IBookingRepository
    {
        private readonly string _xmlFilePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly XmlSerializer _serializer;

        public XmlBookingRepository(string xmlFilePath = "bookings.xml")
        {
            _xmlFilePath = xmlFilePath;
            _serializer = new XmlSerializer(typeof(List<BookingFormModel>));
        }

        public async Task<IEnumerable<BookingFormModel>> GetAllBookingsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_xmlFilePath))
                {
                    return new List<BookingFormModel>();
                }

                using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read);
                // FIXED: Remove Task.Run wrapper - this was causing potential issues
                var bookings = (List<BookingFormModel>)_serializer.Deserialize(stream);
                return bookings ?? new List<BookingFormModel>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<BookingFormModel>> GetBookingsForUserAsync(string username)
        {
            var bookings = await GetAllBookingsAsync();
            return bookings.Where(b => b.Username == username);
        }

        public async Task<BookingFormModel> GetBookingByIdAsync(Guid id)
        {
            var bookings = await GetAllBookingsAsync();
            return bookings.FirstOrDefault(b => b.BookingId == id);
        }

        public async Task<BookingFormModel> AddBookingAsync(BookingFormModel booking)
        {
            await _semaphore.WaitAsync();
            try
            {
                // FIXED: Don't call GetAllBookingsAsync() here as it will try to acquire the same semaphore
                var bookings = await GetAllBookingsInternalAsync();
                
                // Generate new ID if not set
                if (booking.BookingId == Guid.Empty)
                {
                    booking.BookingId = Guid.NewGuid();
                }

                bookings.Add(booking);
                await SaveBookingsInternalAsync(bookings);
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
                // FIXED: Don't call GetAllBookingsAsync() here as it will try to acquire the same semaphore
                var bookings = await GetAllBookingsInternalAsync();
                var existingBooking = bookings.FirstOrDefault(b => b.BookingId == id);
                
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
                    
                    await SaveBookingsInternalAsync(bookings);
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
                // FIXED: Don't call GetAllBookingsAsync() here as it will try to acquire the same semaphore
                var bookings = await GetAllBookingsInternalAsync();
                var booking = bookings.FirstOrDefault(b => b.BookingId == id);
                
                if (booking != null)
                {
                    bookings.Remove(booking);
                    await SaveBookingsInternalAsync(bookings);
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
            var bookings = await GetAllBookingsAsync();
            return bookings.Any(b => b.BookingId == id);
        }

        public async Task<bool> CheckAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int rooms)
        {
            var allRooms = RoomDataHelper.GetDefaultRooms();
            var selectedRoom = allRooms.FirstOrDefault(r => r.RoomName == roomType);
            if (selectedRoom == null)
                return false;

            var bookings = await GetAllBookingsAsync();
            for (var date = checkIn; date <= checkOut; date = date.AddDays(1))
            {
                int booked = bookings
                    .Where(b => b.RoomType == roomType && b.CheckIn <= date && b.CheckOut >= date)
                    .Sum(b => b.NumberOfRooms);
                if (booked + rooms > selectedRoom.NumberOfRooms)
                    return false;
            }
            return true;
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

                // FIXED: Don't call GetAllBookingsAsync() here as it will try to acquire the same semaphore
                var bookings = await GetAllBookingsInternalAsync();
                
                // Check availability for all booking dates in a single transaction
                foreach (var date in bookingDates)
                {
                    int booked = bookings
                        .Where(b => b.RoomType == booking.RoomType && b.CheckIn <= date && b.CheckOut >= date)
                        .Sum(b => b.NumberOfRooms);
                    if (booked + booking.NumberOfRooms > selectedRoom.NumberOfRooms)
                        return (false, $"Not enough rooms available for '{booking.RoomType}' on {date:yyyy-MM-dd}.", null);
                }

                // If we get here, rooms are available for all dates
                var allBookings = bookings.ToList();
                
                // Generate new ID if not set
                if (booking.BookingId == Guid.Empty)
                {
                    booking.BookingId = Guid.NewGuid();
                }

                allBookings.Add(booking);
                await SaveBookingsInternalAsync(allBookings);
                return (true, "Booking created successfully.", booking);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ADDED: Internal method that doesn't use semaphore (assumes caller already has it)
        private async Task<List<BookingFormModel>> GetAllBookingsInternalAsync()
        {
            if (!File.Exists(_xmlFilePath))
            {
                return new List<BookingFormModel>();
            }

            using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read);
            var bookings = (List<BookingFormModel>)_serializer.Deserialize(stream);
            return bookings ?? new List<BookingFormModel>();
        }

        // ADDED: Internal save method that doesn't use Task.Run
        private async Task SaveBookingsInternalAsync(List<BookingFormModel> bookings)
        {
            var directory = Path.GetDirectoryName(_xmlFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = new FileStream(_xmlFilePath, FileMode.Create, FileAccess.Write);
            _serializer.Serialize(stream, bookings);
            await Task.CompletedTask; // Make it async-compatible
        }

        // Keep the original for backward compatibility (though it should use the internal version now)
        private async Task SaveBookingsAsync(List<BookingFormModel> bookings)
        {
            await SaveBookingsInternalAsync(bookings);
        }
    }
}