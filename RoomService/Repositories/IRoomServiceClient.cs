using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
    public interface IRoomServiceClient
    {
        Task<IEnumerable<RoomDetailsDto>> GetAllRoomsAsync();
        Task<RoomDetailsDto> GetRoomByIdAsync(int id);
        Task<RoomDetailsDto> GetRoomByNameAsync(string roomName);
        Task<RoomDetailsDto> CreateRoomAsync(RoomDetailsDto room);
        Task<RoomDetailsDto> UpdateRoomAsync(int id, RoomDetailsDto room);
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> RoomExistsAsync(int id);
        Task<bool> RoomNameExistsAsync(string roomName);
    }
}
