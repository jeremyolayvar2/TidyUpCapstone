using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly ILogger<PricingController> _logger;

        public PricingController(IPricingService pricingService, ILogger<PricingController> logger)
        {
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate the final price for an item based on category and condition
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <param name="conditionId">The condition ID</param>
        /// <returns>Pricing calculation result</returns>
        [HttpGet("Calculate")]
        public IActionResult CalculatePrice(int categoryId, int conditionId)
        {
            try
            {
                if (categoryId <= 0 || conditionId <= 0)
                {
                    return BadRequest(new { error = "Invalid category or condition ID" });
                }

                var adjustedPrice = _pricingService.CalculateAdjustedPrice(categoryId, conditionId);
                var finalPrice = _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

                var result = new
                {
                    categoryId,
                    conditionId,
                    adjustedPrice = Math.Round(adjustedPrice, 2),
                    finalPrice = Math.Round(finalPrice, 2),
                    taxAmount = Math.Round(adjustedPrice - finalPrice, 2),
                    calculatedAt = DateTime.UtcNow
                };

                _logger.LogDebug("Price calculated for Category {CategoryId}, Condition {ConditionId}: {FinalPrice}",
                    categoryId, conditionId, finalPrice);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for Category {CategoryId}, Condition {ConditionId}",
                    categoryId, conditionId);

                return StatusCode(500, new { error = "An error occurred while calculating the price" });
            }
        }

        /// <summary>
        /// Get pricing breakdown details
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <param name="conditionId">The condition ID</param>
        /// <returns>Detailed pricing breakdown</returns>
        [HttpGet("Breakdown")]
        public IActionResult GetPricingBreakdown(int categoryId, int conditionId)
        {
            try
            {
                if (categoryId <= 0 || conditionId <= 0)
                {
                    return BadRequest(new { error = "Invalid category or condition ID" });
                }

                var adjustedPrice = _pricingService.CalculateAdjustedPrice(categoryId, conditionId);
                var finalPrice = _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);
                var taxAmount = adjustedPrice - finalPrice;
                var taxRate = taxAmount / adjustedPrice;

                var result = new
                {
                    categoryId,
                    conditionId,
                    baseCap = GetBaseCapForCategory(categoryId),
                    conditionModifier = GetConditionModifier(conditionId),
                    adjustedPrice = Math.Round(adjustedPrice, 2),
                    taxRate = Math.Round(taxRate * 100, 1), // as percentage
                    taxAmount = Math.Round(taxAmount, 2),
                    finalPrice = Math.Round(finalPrice, 2),
                    breakdown = new
                    {
                        step1 = $"Base price for category: {GetBaseCapForCategory(categoryId):F2}",
                        step2 = $"Condition modifier: {GetConditionModifier(conditionId):+0.0%;-0.0%}",
                        step3 = $"Adjusted price: {adjustedPrice:F2}",
                        step4 = $"Tax rate: {taxRate * 100:F1}%",
                        step5 = $"Final price: {finalPrice:F2}"
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing breakdown for Category {CategoryId}, Condition {ConditionId}",
                    categoryId, conditionId);

                return StatusCode(500, new { error = "An error occurred while getting the pricing breakdown" });
            }
        }

        /// <summary>
        /// Get available price ranges for a category
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <returns>Price range information for the category</returns>
        [HttpGet("Range/{categoryId}")]
        public IActionResult GetPriceRange(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return BadRequest(new { error = "Invalid category ID" });
                }

                var basePrice = GetBaseCapForCategory(categoryId);
                var conditions = new[] { 1, 2, 3, 4, 5 }; // All condition IDs

                var priceRanges = conditions.Select(conditionId =>
                {
                    var adjustedPrice = _pricingService.CalculateAdjustedPrice(categoryId, conditionId);
                    var finalPrice = _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

                    return new
                    {
                        conditionId,
                        conditionName = GetConditionName(conditionId),
                        adjustedPrice = Math.Round(adjustedPrice, 2),
                        finalPrice = Math.Round(finalPrice, 2)
                    };
                }).ToList();

                var result = new
                {
                    categoryId,
                    categoryName = GetCategoryName(categoryId),
                    basePrice,
                    minPrice = priceRanges.Min(p => p.finalPrice),
                    maxPrice = priceRanges.Max(p => p.finalPrice),
                    pricesByCondition = priceRanges
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price range for Category {CategoryId}", categoryId);
                return StatusCode(500, new { error = "An error occurred while getting the price range" });
            }
        }

        #region Helper Methods

        private decimal GetBaseCapForCategory(int categoryId)
        {
            return categoryId switch
            {
                1 => 50,   // Books & Stationery
                2 => 200,  // Electronics & Gadgets
                3 => 80,   // Toys & Games
                4 => 100,  // Home & Kitchen
                5 => 150,  // Furniture
                6 => 180,  // Appliances
                7 => 70,   // Health & Beauty
                8 => 60,   // Crafts & DIY
                9 => 60,   // School & Office
                10 => 90,  // Sentimental Items
                11 => 75,  // Miscellaneous
                _ => 50
            };
        }

        private decimal GetConditionModifier(int conditionId)
        {
            return conditionId switch
            {
                1 => 0.25m,   // Brand New
                2 => 0.15m,   // Like New
                3 => 0.05m,   // Gently Used
                4 => -0.10m,  // Visible Wear
                5 => -0.25m,  // For Repair/Parts
                _ => 0m
            };
        }

        private string GetCategoryName(int categoryId)
        {
            return categoryId switch
            {
                1 => "Books & Stationery",
                2 => "Electronics & Gadgets",
                3 => "Toys & Games",
                4 => "Home & Kitchen",
                5 => "Furniture",
                6 => "Appliances",
                7 => "Health & Beauty",
                8 => "Crafts & DIY",
                9 => "School & Office",
                10 => "Sentimental Items",
                11 => "Miscellaneous",
                _ => "Unknown Category"
            };
        }

        private string GetConditionName(int conditionId)
        {
            return conditionId switch
            {
                1 => "Brand New",
                2 => "Like New",
                3 => "Gently Used",
                4 => "Visible Wear",
                5 => "For Repair/Parts",
                _ => "Unknown Condition"
            };
        }

        #endregion
    }
}