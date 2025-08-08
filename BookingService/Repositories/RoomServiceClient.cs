using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBookingSystem.Models.Room;

namespace HotelBookingSystem.Repositories
{
    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoomServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Room");
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
                var response = await _httpClient.GetAsync($"/api/Room/{id}");
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
                var response = await _httpClient.GetAsync($"/api/Room/search?roomName={Uri.EscapeDataString(roomName)}");
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

        public async Task<bool> CheckRoomAvailabilityAsync(string roomName, DateTime checkIn, DateTime checkOut, int numberOfRooms)
        {
            try
            {
                var queryString = $"?roomName={Uri.EscapeDataString(roomName)}&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}&numberOfRooms={numberOfRooms}";
                var response = await _httpClient.GetAsync($"/api/Room/availability{queryString}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AvailabilityResponse>(content, _jsonOptions);
                return result?.Available ?? false;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to check room availability: {ex.Message}", ex);
            }
        }

        private class AvailabilityResponse
        {
            public bool Available { get; set; }
            public string Message { get; set; }
            public int TotalRooms { get; set; }
            public int RequestedRooms { get; set; }
        }
    }
}
