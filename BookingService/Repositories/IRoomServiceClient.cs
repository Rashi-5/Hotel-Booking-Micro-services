using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBookingSystem.Models.Room;

namespace HotelBookingSystem.Repositories
{
    public interface IRoomServiceClient
    {
        Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync();
        Task<RoomCardViewModel> GetRoomByIdAsync(int id);
        Task<RoomCardViewModel> GetRoomByNameAsync(string roomName);
        Task<bool> CheckRoomAvailabilityAsync(string roomName, DateTime checkIn, DateTime checkOut, int numberOfRooms);
    }
}
