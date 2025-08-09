using Microsoft.AspNetCore.Mvc;
using ChatbotService.Models;
using ChatbotService.Services;

namespace ChatbotService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;
        private readonly ILogger<PredictionController> _logger;

        public PredictionController(IPredictionService predictionService, ILogger<PredictionController> logger)
        {
            _predictionService = predictionService;
            _logger = logger;
        }

        [HttpGet("report")]
        public async Task<ActionResult<PredictionSummary>> GetPredictionReport()
        {
            try
            {
                var report = await _predictionService.GeneratePredictionReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating prediction report");
                return StatusCode(500, "Error generating prediction report");
            }
        }

        [HttpGet("room-frequency")]
        public async Task<ActionResult<Dictionary<string, int>>> GetRoomBookingFrequency()
        {
            try
            {
                var frequency = await _predictionService.GetRoomBookingFrequencyByTypeAsync();
                return Ok(frequency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room booking frequency");
                return StatusCode(500, "Error getting room booking frequency");
            }
        }

        [HttpGet("demand-per-date")]
        public async Task<ActionResult<Dictionary<DateTime, int>>> GetBookingDemandPerDate()
        {
            try
            {
                var demand = await _predictionService.GetBookingDemandPerDateAsync();
                return Ok(demand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking demand per date");
                return StatusCode(500, "Error getting booking demand per date");
            }
        }

        [HttpGet("most-booked-dates")]
        public async Task<ActionResult<List<DateTime>>> GetMostBookedDates([FromQuery] int top = 5)
        {
            try
            {
                var dates = await _predictionService.GetMostBookedDatesAsync(top);
                return Ok(dates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most booked dates");
                return StatusCode(500, "Error getting most booked dates");
            }
        }

        [HttpGet("average-prices")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetAveragePricePerRoom()
        {
            try
            {
                var prices = await _predictionService.GetAveragePricePerRoomAsync();
                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average prices per room");
                return StatusCode(500, "Error getting average prices per room");
            }
        }
    }
}
