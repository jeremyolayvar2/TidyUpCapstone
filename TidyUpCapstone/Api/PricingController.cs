using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly ILogger<PricingController> _logger;
        private readonly IPricingService _pricingService;

        public PricingController(
            ILogger<PricingController> logger,
            IPricingService pricingService)
        {
            _logger = logger;
            _pricingService = pricingService;
        }

        [HttpGet("Calculate")]
        public IActionResult CalculateTokenPrice(int categoryId, int conditionId)
        {
            try
            {
                if (categoryId <= 0 || conditionId <= 0)
                {
                    return BadRequest(new { message = "Invalid category or condition ID" });
                }

                var adjustedPrice = _pricingService.CalculateAdjustedPrice(categoryId, conditionId);
                var finalPrice = _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

                _logger.LogInformation("Price calculation - CategoryId: {CategoryId}, ConditionId: {ConditionId}, AdjustedPrice: {AdjustedPrice}, FinalPrice: {FinalPrice}",
                    categoryId, conditionId, adjustedPrice, finalPrice);

                return Ok(new
                {
                    adjustedPrice = Math.Round(adjustedPrice, 2),
                    finalPrice = Math.Round(finalPrice, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate price for CategoryId: {CategoryId}, ConditionId: {ConditionId}", categoryId, conditionId);
                return StatusCode(500, new { message = "Error calculating token price" });
            }
        }
    }
}