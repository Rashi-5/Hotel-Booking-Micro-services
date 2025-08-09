using ChatbotService.Data;
using ChatbotService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ChatbotService.Services
{
    public class BotLogicService : IChatbotService
    {
        private readonly ChatbotDbContext _context;
        private readonly IBookingServiceClient _bookingService;
        private readonly IRoomServiceClient _roomService;
        private readonly ILogger<BotLogicService> _logger;

        public BotLogicService(
            ChatbotDbContext context,
            IBookingServiceClient bookingService,
            IRoomServiceClient roomService,
            ILogger<BotLogicService> logger)
        {
            _context = context;
            _bookingService = bookingService;
            _roomService = roomService;
            _logger = logger;
        }

        public async Task<ChatResponse> GetBotResponseAsync(ChatRequest request)
        {
            try
            {
                var response = await ProcessUserMessage(request.UserMessage);
                
                // Save chat message to database
                var chatMessage = new ChatMessage
                {
                    UserMessage = request.UserMessage,
                    BotResponse = response,
                    UserId = request.UserId,
                    SessionId = request.SessionId,
                    Timestamp = DateTime.UtcNow
                };
                
                await SaveChatMessageAsync(chatMessage);

                return new ChatResponse
                {
                    BotResponse = response,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return new ChatResponse
                {
                    BotResponse = "I'm sorry, I encountered an error processing your request. Please try again.",
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(string? userId, string? sessionId)
        {
            var query = _context.ChatMessages.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(m => m.UserId == userId);
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(m => m.SessionId == sessionId);
            }

            return await query
                .OrderBy(m => m.Timestamp)
                .Take(10) 
                .ToListAsync();
        }

        public async Task<ChatMessage> SaveChatMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        private async Task<string> ProcessUserMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Please enter a message.";

            string msg = userMessage.Trim().ToLower();

            // Quick intent mapping for exact/close matches
            var intentMap = new Dictionary<string, Func<Task<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "check room availability", GetWeeklyAvailability },
                { "room availability", GetWeeklyAvailability },
                { "availability", GetWeeklyAvailability },
                { "i want to book a room", () => Task.FromResult("Great! You can book a room as a one-time or recurring booking using the booking form.") },
                { "book a room", () => Task.FromResult("Great! You can book a room as a one-time or recurring booking using the booking form.") },
                { "booking", () => Task.FromResult("Great! You can book a room as a one-time or recurring booking using the booking form.") },
                { "show me booking report", GenerateBookingSummary },
                { "report", GenerateBookingSummary },
                { "booking report", GenerateBookingSummary },
                { "room prices", GetRoomPricing },
                { "pricing", GetRoomPricing },
                { "what's the price for rooms?", GetRoomPricing },
                { "what is the price for rooms?", GetRoomPricing },
                { "price predictions", GetPricingTrend },
                { "predictions", GetPricingTrend },
                { "show price predictions", GetPricingTrend },
                { "deluxe amenities", () => GetRoomAmenities("Deluxe Suite") },
                { "standard amenities", () => GetRoomAmenities("Standard Room") },
                { "family amenities", () => GetRoomAmenities("Family Room") },
                { "room amenities", GetAllRoomAmenities },
                { "available rooms", GetAllRoomTypes },
                { "room list", GetAllRoomTypes },
                { "deluxe room", () => GetRoomDetails("Deluxe Suite") },
                { "deluxe suite", () => GetRoomDetails("Deluxe Suite") },
                { "standard room", () => GetRoomDetails("Standard Room") },
                { "family room", () => GetRoomDetails("Family Room") },
                { "room types", GetAllRoomTypes },
                { "hello", () => Task.FromResult("Hello! I'm your hotel booking assistant. How can I help you today?") },
                { "hi", () => Task.FromResult("Hello! I'm your hotel booking assistant. How can I help you today?") },
                { "hey", () => Task.FromResult("Hello! I'm your hotel booking assistant. How can I help you today?") },
                { "thank you", () => Task.FromResult("You're welcome! Is there anything else I can help you with?") },
                { "thanks", () => Task.FromResult("You're welcome! Is there anything else I can help you with?") },
                { "goodbye", () => Task.FromResult("Goodbye! Have a great day!") },
                { "bye", () => Task.FromResult("Goodbye! Have a great day!") },
                { "help", () => Task.FromResult(GetHelpMessage()) },
                { "i need help", () => Task.FromResult(GetHelpMessage()) }
            };

            if (intentMap.TryGetValue(msg, out var action))
            {
                return await action();
            }

            // If no exact match, try keyword-based detection
            return await HandleGeneralQuery(userMessage);
        }

        private async Task<string> GetWeeklyAvailability()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                var rooms = await _roomService.GetAllRoomsAsync();
                var today = DateTime.Today;
                var dates = Enumerable.Range(0, 7).Select(i => today.AddDays(i)).ToList();

                var availability = new Dictionary<DateTime, int>();

                foreach (var date in dates)
                {
                    int totalRooms = rooms.Sum(r => r.NumberOfRooms);
                    int bookedRooms = bookings.Count(b => b.CheckIn.Date <= date.Date && b.CheckOut.Date > date.Date);
                    availability[date] = Math.Max(0, totalRooms - bookedRooms);
                }

                var sb = new StringBuilder();
                sb.AppendLine("Weekly Room Availability");
                sb.AppendLine(new string('─', 35));
                sb.AppendLine($"{"Date",-15}{"Status",-20}");
                sb.AppendLine(new string('─', 35));

                foreach (var kvp in availability)
                {
                    string status = kvp.Value > 0
                        ? $" {kvp.Value} room(s) available"
                        : " Fully booked";

                    sb.AppendLine($"{kvp.Key:ddd, MMM d}".PadRight(15) + status);
                }

                sb.AppendLine(new string('─', 35));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room availability");
                return "Sorry, I'm unable to check room availability right now. Please try again later.";
            }
        }

        private async Task<string> GetRoomPricing()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();

                if (!rooms.Any())
                    return "Sorry, I couldn't retrieve room pricing information right now.";

                var sb = new StringBuilder();
                sb.AppendLine("Room Prices");
                sb.AppendLine(new string('─', 35));
                sb.AppendLine($"{"Room Type",-20}{"Price",-15}");
                sb.AppendLine(new string('─', 35));

                foreach (var room in rooms)
                {
                    sb.AppendLine($"{room.RoomName.PadRight(20)}${room.Price}/night");
                }

                sb.AppendLine(new string('─', 35));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room pricing");
                return "Sorry, I'm unable to get room pricing right now. Please try again later.";
            }
        }


        private async Task<string> GenerateBookingSummary()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                
                if (!bookings.Any())
                    return "There are no bookings available right now.";

                int totalBookings = bookings.Count();
                var groupedByType = bookings.GroupBy(b => b.RoomType)
                                            .Select(g => $"{g.Key}: {g.Count()} booking(s)");

                return $"Booking Report:\nTotal Bookings: {totalBookings}\n" +
                       string.Join("\n", groupedByType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating booking summary");
                return "Sorry, I'm unable to generate a booking summary right now. Please try again later.";
            }
        }

        private async Task<string> GetPricingTrend()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();

                if (!bookings.Any())
                    return "No booking data available to calculate pricing trends.";

                var dailyPrices = bookings
                    .GroupBy(b => b.CheckIn.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        AveragePrice = g.Average(b => b.TotalPrice / Math.Max(1, b.NumberOfRooms))
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                if (dailyPrices.Count < 2)
                    return "Not enough data to determine a pricing trend.";

                var first = dailyPrices.First().AveragePrice;
                var last = dailyPrices.Last().AveragePrice;

                string trend = last > first 
                    ? $"increasing (from {first:C} to {last:C})"
                    : last < first 
                        ? $"decreasing (from {first:C} to {last:C})"
                        : "stable";

                return $"Based on past bookings, the room pricing trend is {trend}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing trend");
                return "Sorry, I'm unable to calculate pricing trends right now. Please try again later.";
            }
        }

        private async Task<string> GetRoomAmenities(string roomType)
        {
            try
            {
                var amenities = await _bookingService.GetRoomAmenitiesAsync(roomType);
                if (amenities.Any())
                {
                    return $"{roomType} Amenities:\n" + string.Join("\n• ", amenities.Prepend(""));
                }
                return $"Sorry, I couldn't find amenities information for {roomType}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting amenities for room type: {RoomType}", roomType);
                return $"Sorry, I'm unable to get amenities for {roomType} right now.";
            }
        }

        private async Task<string> GetAllRoomAmenities()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();
                if (rooms.Any())
                {
                    var amenitiesInfo = rooms.Select(room => 
                        $"{room.RoomName}:\n• {string.Join("\n• ", room.Amenities)}");
                    return "Room Amenities:\n\n" + string.Join("\n\n", amenitiesInfo);
                }
                return "Sorry, I couldn't find room amenities information.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all room amenities");
                return "Sorry, I'm unable to get room amenities right now.";
            }
        }

        private async Task<string> GetAllRoomTypes()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();
                if (rooms.Any())
                {
                    var roomInfo = rooms.Select(room => 
                        $"• {room.RoomName} - ${room.Price}/night ({room.NumberOfRooms} rooms available)");
                    return "Available Room Types:\n" + string.Join("\n", roomInfo);
                }
                return "Sorry, I couldn't find room information.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room types");
                return "Sorry, I'm unable to get room types right now.";
            }
        }

        private async Task<string> GetRoomDetails(string roomName)
        {
            try
            {
                var room = await _roomService.GetRoomByNameAsync(roomName);
                if (room != null)
                {
                    return $"{room.RoomName}\n" +
                           $"Price: ${room.Price}/night\n" +
                           $"Available Rooms: {room.NumberOfRooms}\n" +
                           $"Description: {room.Description}\n" +
                           $"Amenities: {string.Join(", ", room.Amenities)}";
                }
                return $"Sorry, I couldn't find information for {roomName}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room details for: {RoomName}", roomName);
                return $"Sorry, I'm unable to get details for {roomName} right now.";
            }
        }

        private string GetHelpMessage()
        {
            return " I'm here to help! Here's what I can do:\n\n" +
                   "• Check room availability - Ask 'check room availability'\n" +
                   "• Show room prices - Ask 'room prices'\n" +
                   "• Show room types - Ask 'room types'\n" +
                   "• Get room details - Ask 'deluxe room' or 'standard room'\n" +
                   "• Check amenities - Ask 'deluxe amenities' or 'room amenities'\n" +
                   "• Generate booking reports - Ask 'show me a report'\n" +
                   "• Show price predictions - Ask 'price predictions'\n" +
                   "• Help with booking - Ask 'I want to book a room'\n\n" +
                   "Just type your question or select from the quick options!";
        }

        private async Task<string> HandleGeneralQuery(string userMessage)
        {
            var lowerMessage = userMessage.ToLower();

            // Priority intent detection
            if (lowerMessage.Contains("availability") || lowerMessage.Contains("free rooms"))
            {
                return await GetWeeklyAvailability();
            }

            if (lowerMessage.Contains("price") || lowerMessage.Contains("cost") || lowerMessage.Contains("rate"))
            {
                if (lowerMessage.Contains("predict") || lowerMessage.Contains("trend") || lowerMessage.Contains("forecast"))
                {
                    return await GetPricingTrend();
                }
                return await GetRoomPricing();
            }

            if (lowerMessage.Contains("book") || lowerMessage.Contains("reservation"))
            {
                return "I can help you with booking! You can use our booking form to make a reservation. Would you like me to check room availability first?";
            }

            if (lowerMessage.Contains("cancel") || lowerMessage.Contains("modify"))
            {
                return "For booking cancellations or modifications, please contact our support team or use your booking management page.";
            }

            if (lowerMessage.Contains("amenities") || lowerMessage.Contains("facilities"))
            {
                // Check for specific room type amenities
                if (lowerMessage.Contains("deluxe"))
                {
                    return await GetRoomAmenities("Deluxe Suite");
                }
                if (lowerMessage.Contains("standard"))
                {
                    return await GetRoomAmenities("Standard Room");
                }
                if (lowerMessage.Contains("family"))
                {
                    return await GetRoomAmenities("Family Room");
                }
                
                // Default to all room amenities
                return await GetAllRoomAmenities();
            }

            // Check for room information queries
            if (lowerMessage.Contains("deluxe") && (lowerMessage.Contains("room") || lowerMessage.Contains("suite")))
            {
                return await GetRoomDetails("Deluxe Suite");
            }
            
            if (lowerMessage.Contains("standard") && lowerMessage.Contains("room"))
            {
                return await GetRoomDetails("Standard Room");
            }
            
            if (lowerMessage.Contains("family") && lowerMessage.Contains("room"))
            {
                return await GetRoomDetails("Family Room");
            }

            if (lowerMessage.Contains("room") && (lowerMessage.Contains("types") || lowerMessage.Contains("options") || lowerMessage.Contains("list")))
            {
                return await GetAllRoomTypes();
            }

            // Fallback response
            return $"I understand you said: \"{userMessage}\"\n\n" +
                "I'm still learning! You can try asking about:\n" +
                "• Room availability\n• Room prices\n• Booking reports\n• Price predictions\n\n" +
                "Or type 'help' for more options.";
        }
    }
}
