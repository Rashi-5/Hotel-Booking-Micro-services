using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Repositories;
using HotelBookingSystem.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HotelBookingSystem.Services
{
    public class BookingManager
    {
        private readonly IBookingRepository _repository;
        private readonly IRoomServiceClient _roomServiceClient;
        private readonly IConfiguration _configuration;

        public BookingManager(IBookingRepository repository, IRoomServiceClient roomServiceClient, IConfiguration configuration)
        {
            _repository = repository;
            _roomServiceClient = roomServiceClient;
            _configuration = configuration;
        }

        public async Task<IEnumerable<BookingFormModel>> GetAllBookingsAsync()
        {
            return await _repository.GetAllBookingsAsync();
        }

        public async Task<IEnumerable<BookingFormModel>> GetBookingsForUserAsync(string username)
        {
            return await _repository.GetBookingsForUserAsync(username);
        }

        public async Task<BookingFormModel> GetBookingByIdAsync(Guid id)
        {
            return await _repository.GetBookingByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Guid? BookingId)> CreateBookingAsync(BookingFormModel model)
        {
            if (model == null)
                return (false, "Invalid booking data.", null);
            if (model.CheckIn == default || model.CheckOut == default)
                return (false, "Check-in and check-out dates are required.", null);

            // Get room information from Room Service
            var selectedRoom = await _roomServiceClient.GetRoomByNameAsync(model.RoomType);
            if (selectedRoom == null)
                return (false, $"Room type '{model.RoomType}' not found.", null);

            // Check room availability through Room Service
            bool isAvailable = await _roomServiceClient.CheckRoomAvailabilityAsync(model.RoomType, model.CheckIn, model.CheckOut, model.NumberOfRooms);
            if (!isAvailable)
                return (false, $"Not enough rooms available for '{model.RoomType}'.", null);

            // Calculate booking dates
            List<DateTime> bookingDates = new List<DateTime>();
            if (model.BookingType == "Recurring")
            {
                int interval = model.Interval ?? 1;
                if (model.Frequency == "Daily")
                {
                    for (var date = model.CheckIn; date <= model.CheckOut; date = date.AddDays(interval))
                        bookingDates.Add(date);
                }
                else if (model.Frequency == "Weekly" && model.Days != null)
                {
                    var selectedDays = model.Days.Select(d => Enum.Parse<DayOfWeek>(d)).ToList();
                    bookingDates = GetRecurringBookingDates(model.CheckIn, model.CheckOut, selectedDays, interval);
                }
                else if (model.Frequency == "Monthly")
                {
                    var date = model.CheckIn;
                    while (date <= model.CheckOut)
                    {
                        bookingDates.Add(date);
                        date = date.AddMonths(interval);
                    }
                }
            }
            else
            {
                bookingDates.Add(model.CheckIn);
            }

            // Check room availability for each date through local repository
            foreach (var date in bookingDates)
            {
                bool available = await _repository.CheckAvailabilityAsync(model.RoomType, date, date, model.NumberOfRooms);
                if (!available)
                    return (false, $"Not enough rooms available for '{model.RoomType}' on {date:yyyy-MM-dd}.", null);
            }

            // Set default amenities if not provided
            if (model.Amenities == null || !model.Amenities.Any())
            {
                model.Amenities = selectedRoom.Amenities ?? new List<string>();
            }

            // Calculate total price
            decimal pricePerRoom = decimal.TryParse(selectedRoom.Price, out var p) ? p : 0;
            decimal totalPrice;

            if (model.BookingType == "Recurring")
            {
                // For recurring bookings, multiply by number of booking dates
                totalPrice = pricePerRoom * model.NumberOfRooms * bookingDates.Count;
            }
            else
            {
                // For single bookings, calculate based on actual duration
                int numberOfDays = (model.CheckOut - model.CheckIn).Days;
                if (numberOfDays <= 0) numberOfDays = 1; // Minimum 1 day
                totalPrice = pricePerRoom * model.NumberOfRooms * numberOfDays;
            }

            model.TotalPrice = totalPrice;
            model.BookingId = Guid.NewGuid();

            try
            {
                // Use the new method that handles availability checking and booking creation in a single transaction
                var result = await _repository.CreateBookingWithAvailabilityCheckAsync(model, bookingDates);
                if (result.Success)
                {
                    return (true, result.Message, result.Booking.BookingId);
                }
                else
                {
                    return (false, result.Message, null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error creating booking: {ex.Message}", null);
            }
        }

        public async Task<bool> UpdateBookingAsync(Guid id, BookingFormModel updated)
        {
            var existing = await _repository.GetBookingByIdAsync(id);
            if (existing == null)
                return false;

            try
            {
                var result = await _repository.UpdateBookingAsync(id, updated);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBookingAsync(Guid id)
        {
            return await _repository.DeleteBookingAsync(id);
        }

        public async Task<bool> CheckAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int rooms)
        {
            // First check with Room Service
            bool roomServiceAvailable = await _roomServiceClient.CheckRoomAvailabilityAsync(roomType, checkIn, checkOut, rooms);
            if (!roomServiceAvailable)
                return false;

            // Then check with local repository for existing bookings
            return await _repository.CheckAvailabilityAsync(roomType, checkIn, checkOut, rooms);
        }

        public async Task<IEnumerable<RoomDetailsDto>> GetAllRoomsAsync()
        {
            return await _roomServiceClient.GetAllRoomsAsync();
        }

        public async Task<RoomDetailsDto> GetRoomByNameAsync(string roomName)
        {
            return await _roomServiceClient.GetRoomByNameAsync(roomName);
        }

        public string GetStorageType()
        {
            return _configuration["Storage:Type"] ?? "Database";
        }

        private static List<DateTime> GetRecurringBookingDates(
            DateTime startDate,
            DateTime endDate,
            List<DayOfWeek> selectedDays,
            int intervalWeeks = 1)
        {
            var result = new List<DateTime>();
            DateTime firstWeekStart = startDate.Date;
            firstWeekStart = firstWeekStart.AddDays(-(int)firstWeekStart.DayOfWeek + (int)DayOfWeek.Monday);
            if (firstWeekStart > startDate) firstWeekStart = startDate;
            for (DateTime weekStart = firstWeekStart; weekStart <= endDate; weekStart = weekStart.AddDays(7 * intervalWeeks))
            {
                foreach (var day in selectedDays)
                {
                    var candidate = weekStart.AddDays((int)day - (int)weekStart.DayOfWeek);
                    if (candidate < startDate)
                        candidate = candidate.AddDays(7);
                    if (candidate >= startDate && candidate <= endDate && !result.Contains(candidate))
                    {
                        result.Add(candidate);
                    }
                }
            }
            result.Sort();
            return result;
        }
    }
} 