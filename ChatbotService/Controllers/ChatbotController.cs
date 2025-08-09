using Microsoft.AspNetCore.Mvc;
using ChatbotService.Models;
using ChatbotService.Services;

namespace ChatbotService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserMessage))
                {
                    return BadRequest(new ChatResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "User message is required",
                        BotResponse = "Please enter a message."
                    });
                }

                var response = await _chatbotService.GetBotResponseAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new ChatResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Internal server error",
                    BotResponse = "I'm sorry, something went wrong. Please try again."
                });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<ChatMessage>>> GetChatHistory(
            [FromQuery] string? userId, 
            [FromQuery] string? sessionId)
        {
            try
            {
                var history = await _chatbotService.GetChatHistoryAsync(userId, sessionId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history");
                return StatusCode(500, "Error retrieving chat history");
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "ChatbotService API is working!",
                timestamp = DateTime.UtcNow,
                availableEndpoints = new[] {
                    "POST /api/chatbot/ask - Send a message to the chatbot",
                    "GET /api/chatbot/history - Get chat history",
                    "GET /api/prediction/report - Get prediction report",
                    "GET /api/chatbot/test - Test endpoint"
                }
            });
        }
    }
}
