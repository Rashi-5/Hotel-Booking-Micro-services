using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
    public interface IRoomServiceClient
    {
        Task<IEnumerable<RoomDetailsDto>> GetAllRoomsAsync();
        Task<RoomDetailsDto> GetRoomByIdAsync(int id);
        Task<RoomDetailsDto> GetRoomByNameAsync(string roomName);
        Task<bool> CheckRoomAvailabilityAsync(string roomName, DateTime checkIn, DateTime checkOut, int numberOfRooms);
    }

    public class RoomDetailsDto
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<string> Amenities { get; set; }
        public bool isDefault { get; set; }
        public string Price { get; set; }
        public int NumberOfRooms { get; set; }
    }
}
