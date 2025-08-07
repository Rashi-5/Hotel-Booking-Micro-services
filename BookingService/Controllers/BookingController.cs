using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Services;
using HotelBookingSystem.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace HotelBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingManager _bookingManager;
        
        public BookingController(BookingManager bookingManager)
        {
            _bookingManager = bookingManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingFormModel model)
        {
            // Set default amenities if not provided
            if (model.Amenities == null || !model.Amenities.Any())
            {
                var selectedRoom = await _bookingManager.GetRoomByNameAsync(model.RoomType);
                model.Amenities = selectedRoom?.Amenities ?? new List<string>();
            }
            
            var result = await _bookingManager.CreateBookingAsync(model);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message, bookingId = result.BookingId });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability(string roomType, DateTime checkIn, DateTime checkOut, int rooms)
        {
            var available = await _bookingManager.CheckAvailabilityAsync(roomType, checkIn, checkOut, rooms);
            var message = available ? "Rooms are available for the selected dates." : "Not enough rooms available for the selected dates.";
            return Ok(new { available, message });
        }

        [HttpGet("rooms")]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _bookingManager.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("rooms/{roomType}/amenities")]
        public async Task<IActionResult> GetRoomAmenities(string roomType)
        {
            var selectedRoom = await _bookingManager.GetRoomByNameAsync(roomType);
            if (selectedRoom == null)
                return NotFound(new { message = "Room type not found." });
            return Ok(new { roomType, amenities = selectedRoom.Amenities });
        }

        [HttpGet("test")]
        public IActionResult TestEndpoint()
        {
            return Ok(new { 
                message = "BookingService API is working!",
                availableEndpoints = new[] {
                    "POST /api/booking - Create a new booking",
                    "GET /api/booking/availability - Check room availability",
                    "GET /api/booking/rooms - Get all rooms",
                    "GET /api/booking/rooms/{roomType}/amenities - Get room amenities",
                    "GET /api/booking/user/{username} - Get user bookings",
                    "GET /api/booking/all - Get all bookings",
                    "GET /api/booking/{id} - Get booking by ID",
                    "PUT /api/booking/{id} - Update booking",
                    "DELETE /api/booking/{id} - Delete booking",
                    "GET /api/booking/storage-type - Get storage type"
                }
            });
        }

        [Authorize]
        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetBookingsForUser(string username)
        {
            var userBookings = await _bookingManager.GetBookingsForUserAsync(username);
            return Ok(userBookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _bookingManager.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await _bookingManager.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();
            return Ok(booking);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var success = await _bookingManager.DeleteBookingAsync(id);
            if (!success)
                return NotFound();
            return Ok(new { message = "Booking deleted successfully." });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] BookingFormModel updated)
        {
            var success = await _bookingManager.UpdateBookingAsync(id, updated);
            if (!success)
                return NotFound();
            return Ok(new { message = "Booking updated successfully." });
        }

        [HttpGet("storage-type")]
        public IActionResult GetStorageType()
        {
            var storageType = _bookingManager.GetStorageType();
            return Ok(new { StorageType = storageType });
        }
    }
}
