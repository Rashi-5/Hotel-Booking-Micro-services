namespace ChatbotService.Models
{
    public class PredictionSummary
    {
        public Dictionary<string, int> MostBookedRoomTypes { get; set; } = new();
        
        public List<DateTime> MostDemandedDates { get; set; } = new();
        
        public Dictionary<string, decimal> AverageRoomPricePerType { get; set; } = new();
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
