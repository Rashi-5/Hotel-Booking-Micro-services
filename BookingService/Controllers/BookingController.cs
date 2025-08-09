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
using System.Linq;

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

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingFormModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                return BadRequest("Username is required.");

            try
            {
                // Ensure arrays are not null
                if (model.Days == null)
                {
                    model.Days = new List<string>();
                }
                
                var (success, message, bookingId) = await _bookingManager.CreateBookingAsync(model);
                if (success)
                {
                    return Ok(new { message, bookingId });
                }
                else
                {
                    return BadRequest(new { message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error creating booking: {ex.Message}" });
            }
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

        [Authorize]
        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetBookingsForUser(string username)
        {
            var userBookings = await _bookingManager.GetBookingsForUserAsync(username);
            return Ok(userBookings);
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _bookingManager.GetAllBookingsAsync();
            return Ok(bookings);
        }

        // Chatbot endpoint without authentication
        [HttpGet("chatbot/all")]
        public async Task<IActionResult> GetAllBookingsForChatbot()
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
            try
            {
                // Ensure arrays are not null
                if (updated.Days == null)
                {
                    updated.Days = new List<string>();
                }
                
                var success = await _bookingManager.UpdateBookingAsync(id, updated);
                if (!success)
                    return NotFound();
                return Ok(new { message = "Booking updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating booking: {ex.Message}" });
            }
        }

        [HttpGet("storage-type")]
        public IActionResult GetStorageType()
        {
            var storageType = _bookingManager.GetStorageType();
            return Ok(new { StorageType = storageType });
        }
    }
}
