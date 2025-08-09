using ChatbotService.Models;

namespace ChatbotService.Services
{
    public interface IPredictionService
    {
        Task<PredictionSummary> GeneratePredictionReportAsync();
        Task<Dictionary<string, int>> GetRoomBookingFrequencyByTypeAsync();
        Task<Dictionary<DateTime, int>> GetBookingDemandPerDateAsync();
        Task<List<DateTime>> GetMostBookedDatesAsync(int top = 5);
        Task<Dictionary<string, decimal>> GetAveragePricePerRoomAsync();
    }
}
