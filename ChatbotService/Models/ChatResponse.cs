namespace ChatbotService.Models
{
    public class ChatResponse
    {
        public string BotResponse { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool Success { get; set; } = true;
        
        public string? ErrorMessage { get; set; }
    }
}
