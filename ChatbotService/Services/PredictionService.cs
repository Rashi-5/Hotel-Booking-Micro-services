using ChatbotService.Models;

namespace ChatbotService.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly IBookingServiceClient _bookingService;
        private readonly ILogger<PredictionService> _logger;

        public PredictionService(IBookingServiceClient bookingService, ILogger<PredictionService> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public async Task<PredictionSummary> GeneratePredictionReportAsync()
        {
            try
            {
                return new PredictionSummary
                {
                    MostBookedRoomTypes = await GetRoomBookingFrequencyByTypeAsync(),
                    MostDemandedDates = await GetMostBookedDatesAsync(),
                    AverageRoomPricePerType = await GetAveragePricePerRoomAsync(),
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating prediction report");
                return new PredictionSummary();
            }
        }

        public async Task<Dictionary<string, int>> GetRoomBookingFrequencyByTypeAsync()
        {
            try
            {
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var frequency = new Dictionary<string, int>();

                foreach (var booking in allBookings)
                {
                    if (!frequency.ContainsKey(booking.RoomType))
                        frequency[booking.RoomType] = 0;

                    frequency[booking.RoomType]++;
                }

                return frequency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room booking frequency");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<DateTime, int>> GetBookingDemandPerDateAsync()
        {
            try
            {
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var demand = new Dictionary<DateTime, int>();

                foreach (var booking in allBookings)
                {
                    var date = booking.CheckIn.Date;
                    if (!demand.ContainsKey(date))
                        demand[date] = 0;

                    demand[date] += booking.NumberOfRooms;
                }

                return demand;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking demand per date");
                return new Dictionary<DateTime, int>();
            }
        }

        public async Task<List<DateTime>> GetMostBookedDatesAsync(int top = 5)
        {
            try
            {
                var demand = await GetBookingDemandPerDateAsync();
                return demand
                    .OrderByDescending(x => x.Value)
                    .Take(top)
                    .Select(x => x.Key)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most booked dates");
                return new List<DateTime>();
            }
        }

        public async Task<Dictionary<string, decimal>> GetAveragePricePerRoomAsync()
        {
            try
            {
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var priceMap = new Dictionary<string, List<decimal>>();

                foreach (var booking in allBookings)
                {
                    var days = Math.Max(1, (booking.CheckOut - booking.CheckIn).Days);
                    decimal pricePerRoom = booking.TotalPrice / (booking.NumberOfRooms * days);

                    if (!priceMap.ContainsKey(booking.RoomType))
                        priceMap[booking.RoomType] = new List<decimal>();

                    priceMap[booking.RoomType].Add(pricePerRoom);
                }

                return priceMap.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Math.Round(kvp.Value.Average(), 2)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average price per room");
                return new Dictionary<string, decimal>();
            }
        }
    }
}
