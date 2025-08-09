using ChatbotService.Models;

namespace ChatbotService.Services
{
    public interface IRoomServiceClient
    {
        Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync();
        Task<RoomCardViewModel?> GetRoomByNameAsync(string roomName);
        Task<RoomCardViewModel?> GetRoomByIdAsync(int id);
    }
}
