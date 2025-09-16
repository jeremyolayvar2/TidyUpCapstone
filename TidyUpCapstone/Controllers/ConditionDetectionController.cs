using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IVertexAiService _vertexAIService;
        private readonly ILogger<AIController> _logger;

        public AIController(IVertexAiService vertexAIService, ILogger<AIController> logger)
        {
            _vertexAIService = vertexAIService;
            _logger = logger;
        }

        [HttpPost("DetectCondition")]
        public async Task<IActionResult> DetectCondition(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Image file is required" });
                }

                // Use the correct method name from your existing service
                var result = await _vertexAIService.PredictConditionAsync(imageFile);

                return Ok(new
                {
                    success = result.Success,
                    conditionId = result.SuggestedConditionId,
                    conditionName = GetConditionName(result.SuggestedConditionId),
                    confidence = result.ConfidenceScore,
                    message = result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting condition from image");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error during condition detection"
                });
            }
        }

        private string GetConditionName(int conditionId)
        {
            return conditionId switch
            {
                1 => "Excellent",
                3 => "Good", // Note: Your condition mapping uses 3 for Good
                4 => "Fair", // Note: Your condition mapping uses 4 for Fair
                _ => "Unknown"
            };
        }
    }
}