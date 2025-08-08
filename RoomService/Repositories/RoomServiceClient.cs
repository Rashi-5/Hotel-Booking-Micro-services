using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBookingSystem.Models.Room;

namespace HotelBookingSystem.Repositories
{
    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl;

        public RoomServiceClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<RoomCardViewModel>>(content, _jsonOptions) ?? new List<RoomCardViewModel>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get rooms: {ex.Message}", ex);
            }
        }

        public async Task<RoomCardViewModel> GetRoomByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomCardViewModel>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get room {id}: {ex.Message}", ex);
            }
        }

        public async Task<RoomCardViewModel> GetRoomByNameAsync(string roomName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room/search?roomName={Uri.EscapeDataString(roomName)}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomCardViewModel>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get room {roomName}: {ex.Message}", ex);
            }
        }

        public async Task<RoomCardViewModel> CreateRoomAsync(RoomCardViewModel room)
        {
            try
            {
                var json = JsonSerializer.Serialize(room, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/room", content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomCardViewModel>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to create room: {ex.Message}", ex);
            }
        }

        public async Task<RoomCardViewModel> UpdateRoomAsync(int id, RoomCardViewModel room)
        {
            try
            {
                var json = JsonSerializer.Serialize(room, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_baseUrl}/api/room/{id}", content);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomCardViewModel>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update room {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/room/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to delete room {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> RoomExistsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> RoomNameExistsAsync(string roomName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room/search?roomName={Uri.EscapeDataString(roomName)}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
