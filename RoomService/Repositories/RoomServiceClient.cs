using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
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

    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoomServiceClient(HttpClient httpClient, string baseUrl = "https://localhost:5238")
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<RoomDetailsDto>> GetAllRoomsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<RoomDetailsDto>>(content, _jsonOptions) ?? new List<RoomDetailsDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get rooms: {ex.Message}", ex);
            }
        }

        public async Task<RoomDetailsDto> GetRoomByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomDetailsDto>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get room {id}: {ex.Message}", ex);
            }
        }

        public async Task<RoomDetailsDto> GetRoomByNameAsync(string roomName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room?roomName={Uri.EscapeDataString(roomName)}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomDetailsDto>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get room {roomName}: {ex.Message}", ex);
            }
        }

        public async Task<RoomDetailsDto> CreateRoomAsync(RoomDetailsDto room)
        {
            try
            {
                var json = JsonSerializer.Serialize(room, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/room", content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RoomDetailsDto>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to create room: {ex.Message}", ex);
            }
        }

        public async Task<RoomDetailsDto> UpdateRoomAsync(int id, RoomDetailsDto room)
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
                return JsonSerializer.Deserialize<RoomDetailsDto>(responseContent, _jsonOptions);
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
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/room?roomName={Uri.EscapeDataString(roomName)}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
