using HotelBookingSystem.Models.Room;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync();
        Task<RoomCardViewModel> GetRoomByIdAsync(int id);
        Task<RoomCardViewModel> GetRoomByNameAsync(string roomName);
        Task<RoomCardViewModel> AddRoomAsync(RoomCardViewModel room);
        Task<RoomCardViewModel> UpdateRoomAsync(RoomCardViewModel room);
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> RoomExistsAsync(int id);
        Task<bool> RoomNameExistsAsync(string roomName);
    }
} 