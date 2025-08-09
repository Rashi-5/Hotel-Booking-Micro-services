using ChatbotService.Models;

namespace ChatbotService.Services
{
    public interface IChatbotService
    {
        Task<ChatResponse> GetBotResponseAsync(ChatRequest request);
        Task<List<ChatMessage>> GetChatHistoryAsync(string? userId, string? sessionId);
        Task<ChatMessage> SaveChatMessageAsync(ChatMessage message);
    }
}
