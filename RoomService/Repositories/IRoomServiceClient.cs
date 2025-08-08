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
        Task<RoomCardViewModel> CreateRoomAsync(RoomCardViewModel room);
        Task<RoomCardViewModel> UpdateRoomAsync(int id, RoomCardViewModel room);
    }
}
