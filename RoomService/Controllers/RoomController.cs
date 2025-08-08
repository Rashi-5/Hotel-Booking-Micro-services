using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models.Room;
using HotelBookingSystem.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly RoomManager _roomManager;
        
        public RoomController(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        // GET: api/room
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRooms()
        {
            var rooms = await _roomManager.GetAllRoomsAsync();
            var result = rooms.Select(r => new {
                r.Id,
                r.RoomName,
                r.ImageUrl,
                r.Description,
                Amenities = r.Amenities ?? new List<string>(),
                r.isDefault,
                r.Price,
                r.NumberOfRooms
            });
            return Ok(result);
        }

        // GET: api/room/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRoomById(int id)
        {
            var r = await _roomManager.GetRoomByIdAsync(id);
            if (r == null) return NotFound();
            
            return Ok(new {
                r.Id,
                r.RoomName,
                r.ImageUrl,
                r.Description,
                Amenities = r.Amenities ?? new List<string>(),
                r.isDefault,
                r.Price,
                r.NumberOfRooms
            });
        }

        // GET: api/room/search?roomName={roomName}
        [HttpGet("search")]
        public async Task<ActionResult<object>> GetRoomByName([FromQuery] string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
                return BadRequest("Room name is required.");
                
            var r = await _roomManager.GetRoomByNameAsync(roomName);
            if (r == null) return NotFound();
            
            return Ok(new {
                r.Id,
                r.RoomName,
                r.ImageUrl,
                r.Description,
                Amenities = r.Amenities ?? new List<string>(),
                r.isDefault,
                r.Price,
                r.NumberOfRooms
            });
        }

        // GET: api/room/availability?roomName={roomName}&checkIn={checkIn}&checkOut={checkOut}&numberOfRooms={numberOfRooms}
        [HttpGet("availability")]
        public async Task<ActionResult<object>> CheckAvailability(
            [FromQuery] string roomName,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            [FromQuery] int numberOfRooms = 1)
        {
            if (string.IsNullOrWhiteSpace(roomName))
                return BadRequest("Room name is required.");

            var room = await _roomManager.GetRoomByNameAsync(roomName);
            if (room == null)
                return NotFound(new { message = "Room type not found." });

            // Check if we have enough rooms available
            if (room.NumberOfRooms < numberOfRooms)
                return Ok(new { 
                    available = false, 
                    message = $"Not enough rooms available. Only {room.NumberOfRooms} rooms available for '{roomName}'." 
                });

            // For now, we'll do a simple check based on total room count
            // In a real scenario, you'd want to check existing bookings for the room type and date range
            bool isAvailable = room.NumberOfRooms >= numberOfRooms;
            
            return Ok(new { 
                available = isAvailable, 
                message = isAvailable ? 
                    $"Rooms are available for the selected dates." : 
                    $"Not enough rooms available for '{roomName}'.",
                totalRooms = room.NumberOfRooms,
                requestedRooms = numberOfRooms
            });
        }

        // POST: api/room
        [HttpPost]
        public async Task<ActionResult> CreateRoom([FromForm] RoomCardViewModel model, [FromForm] string[] amenities)
        {
            if (string.IsNullOrWhiteSpace(model.RoomName))
                return BadRequest("Room name is required.");

            try
            {
                // Handle amenities from form data
                if (amenities != null && amenities.Length > 0)
                {
                    model.Amenities = amenities.ToList();
                }
                else if (model.Amenities == null)
                {
                    model.Amenities = new List<string>();
                }
                
                // Convert RoomDetailsDto to RoomCardViewModel for the manager
                var roomCardViewModel = new RoomCardViewModel
                {
                    RoomName = model.RoomName,
                    ImageUrl = model.ImageUrl,
                    Description = model.Description,
                    Amenities = model.Amenities,
                    isDefault = model.isDefault,
                    Price = model.Price,
                    NumberOfRooms = model.NumberOfRooms
                };
                
                var result = await _roomManager.AddRoomAsync(roomCardViewModel);
                return CreatedAtAction(nameof(GetRoomById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/room/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRoom(int id, [FromForm] RoomCardViewModel model, [FromForm] string[] amenities)
        {
            try
            {
                model.Id = id;
                
                // Handle amenities from form data
                if (amenities != null && amenities.Length > 0)
                {
                    model.Amenities = amenities.ToList();
                }
                else if (model.Amenities == null)
                {
                    model.Amenities = new List<string>();
                }
                
                var roomCardViewModel = new RoomCardViewModel
                {
                    Id = id, // Use the route parameter id
                    RoomName = model.RoomName,
                    ImageUrl = model.ImageUrl,
                    Description = model.Description,
                    Amenities = model.Amenities,
                    isDefault = model.isDefault,
                    Price = model.Price,
                    NumberOfRooms = model.NumberOfRooms
                };

                var result = await _roomManager.UpdateRoomAsync(roomCardViewModel);
                if (result == null)
                    return NotFound();
                    
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/room/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoom(int id)
        {
            try
            {
                var success = await _roomManager.DeleteRoomAsync(id);
                if (!success)
                    return NotFound();
                    
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/room/storage-type
        [HttpGet("storage-type")]
        public ActionResult GetStorageType()
        {
            var storageType = _roomManager.GetStorageType();
            return Ok(new { StorageType = storageType });
        }
    }
}

