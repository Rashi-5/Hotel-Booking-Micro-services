using System.ComponentModel.DataAnnotations;

namespace ChatbotService.Models
{
    public class ChatRequest
    {
        [Required]
        public string UserMessage { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? SessionId { get; set; }
    }
}
