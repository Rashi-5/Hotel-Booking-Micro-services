using ChatbotService.Models;
using System.Text.Json;

namespace ChatbotService.Services
{
    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RoomServiceClient> _logger;

        public RoomServiceClient(HttpClient httpClient, ILogger<RoomServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Room");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<RoomCardViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return rooms ?? new List<RoomCardViewModel>();
                }
                
                _logger.LogWarning("Failed to get rooms: {StatusCode}", response.StatusCode);
                return new List<RoomCardViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all rooms");
                return new List<RoomCardViewModel>();
            }
        }

        public async Task<RoomCardViewModel?> GetRoomByNameAsync(string roomName)
        {
            try
            {
            
                var response = await _httpClient.GetAsync($"/api/Room/search?roomName={Uri.EscapeDataString(roomName)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var room = JsonSerializer.Deserialize<RoomCardViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return room;
                }
                
                _logger.LogWarning("Failed to get room by name '{RoomName}': {StatusCode}", roomName, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room by name: {RoomName}", roomName);
                return null;
            }
        }

        public async Task<RoomCardViewModel?> GetRoomByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Room/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var room = JsonSerializer.Deserialize<RoomCardViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return room;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room by ID: {Id}", id);
                return null;
            }
        }
    }
}
