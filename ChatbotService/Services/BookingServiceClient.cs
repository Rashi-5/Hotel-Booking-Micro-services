using ChatbotService.Models;
using System.Text.Json;

namespace ChatbotService.Services
{
    public class BookingServiceClient : IBookingServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookingServiceClient> _logger;

        public BookingServiceClient(HttpClient httpClient, ILogger<BookingServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<BookingFormModel>> GetAllBookingsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Booking/chatbot/all");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bookings = JsonSerializer.Deserialize<List<BookingFormModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return bookings ?? new List<BookingFormModel>();
                }
                
                _logger.LogWarning("Failed to get bookings from chatbot endpoint: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return new List<BookingFormModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all bookings from chatbot endpoint");
                return new List<BookingFormModel>();
            }
        }

        public async Task<IEnumerable<BookingFormModel>> GetBookingsForUserAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Booking/user/{username}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bookings = JsonSerializer.Deserialize<List<BookingFormModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return bookings ?? new List<BookingFormModel>();
                }
                
                _logger.LogWarning("Failed to get user bookings: {StatusCode}", response.StatusCode);
                return new List<BookingFormModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings for {Username}", username);
                return new List<BookingFormModel>();
            }
        }

        public async Task<bool> CheckRoomAvailabilityAsync(string roomType, DateTime checkIn, DateTime checkOut, int numberOfRooms)
        {
            try
            {
                var url = $"/api/Booking/availability?roomType={roomType}&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}&numberOfRooms={numberOfRooms}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(json);
                    return result.TryGetProperty("available", out var available) && available.GetBoolean();
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room availability");
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetRoomAmenitiesAsync(string roomType)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Booking/rooms/{Uri.EscapeDataString(roomType)}/amenities");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (result != null && result.ContainsKey("amenities"))
                    {
                        var amenitiesJson = result["amenities"].ToString();
                        var amenities = JsonSerializer.Deserialize<List<string>>(amenitiesJson ?? "[]", new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return amenities ?? new List<string>();
                    }
                }
                
                _logger.LogWarning("Failed to get room amenities for '{RoomType}': {StatusCode}", roomType, response.StatusCode);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room amenities for room type: {RoomType}", roomType);
                return new List<string>();
            }
        }
    }
}
