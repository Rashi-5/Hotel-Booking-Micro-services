using System.ComponentModel.DataAnnotations;

namespace ChatbotService.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string UserMessage { get; set; } = string.Empty;
        
        public string BotResponse { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? UserId { get; set; }
        
        public string? SessionId { get; set; }
    }
}
