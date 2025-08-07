using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelBookingSystem.Repositories
{
    public class BookingDetailsDto
    {
        public Guid BookingId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Username { get; set; }
        public string Note { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal TotalPrice { get; set; }
        public string RoomType { get; set; }
        public int Adult { get; set; }
        public int Children { get; set; }
        public string BookingType { get; set; }
        public string Frequency { get; set; }
        public int? Interval { get; set; }
        public List<string> Days { get; set; }
        public List<string> Amenities { get; set; }
    }

    public class BookingServiceClient : IBookingServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookingServiceClient(HttpClient httpClient, string baseUrl = "https://localhost:7283")
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<BookingDetailsDto>> GetAllBookingsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/booking/all");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<BookingDetailsDto>>(content, _jsonOptions) ?? new List<BookingDetailsDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get bookings: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BookingDetailsDto>> GetBookingsForUserAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/booking/user/{Uri.EscapeDataString(username)}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<BookingDetailsDto>>(content, _jsonOptions) ?? new List<BookingDetailsDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get bookings for user {username}: {ex.Message}", ex);
            }
        }

        public async Task<BookingDetailsDto> GetBookingByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/booking/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<BookingDetailsDto>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get booking {id}: {ex.Message}", ex);
            }
        }

        public async Task<BookingDetailsDto> CreateBookingAsync(BookingDetailsDto booking)
        {
            try
            {
                var json = JsonSerializer.Serialize(booking, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/booking", content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<BookingDetailsDto>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to create booking: {ex.Message}", ex);
            }
        }

        public async Task<BookingDetailsDto> UpdateBookingAsync(Guid id, BookingDetailsDto booking)
        {
            try
            {
                var json = JsonSerializer.Serialize(booking, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_baseUrl}/api/booking/{id}", content);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<BookingDetailsDto>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update booking {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteBookingAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/booking/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to delete booking {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> BookingExistsAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/booking/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> CheckAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int rooms)
        {
            try
            {
                var queryString = $"?roomType={Uri.EscapeDataString(roomType)}&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}&rooms={rooms}";
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/booking/availability{queryString}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AvailabilityResponse>(content, _jsonOptions);
                return result?.Available ?? false;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to check availability: {ex.Message}", ex);
            }
        }

        private class AvailabilityResponse
        {
            public bool Available { get; set; }
            public string Message { get; set; }
        }
    }
} 