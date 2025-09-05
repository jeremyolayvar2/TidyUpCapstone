using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisionController : ControllerBase
    {
        private readonly IVisionService _visionService;
        private readonly ILogger<VisionController> _logger;

        public VisionController(IVisionService visionService, ILogger<VisionController> logger)
        {
            _visionService = visionService;
            _logger = logger;
        }

        /// <summary>
        /// Analyze an image to suggest appropriate category
        /// </summary>
        /// <param name="imageFile">Image file to analyze</param>
        /// <returns>Analysis result with suggested category</returns>
        [HttpPost("AnalyzeImage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No image file provided" });
                }

                // Validate file size (10MB limit)
                const long maxFileSize = 10 * 1024 * 1024;
                if (imageFile.Length > maxFileSize)
                {
                    return BadRequest(new { success = false, message = "Image file too large. Maximum size is 10MB." });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    return BadRequest(new { success = false, message = "Invalid file type. Please use JPEG, PNG, or WebP." });
                }

                _logger.LogInformation("Analyzing image: {FileName}, Size: {Size} bytes",
                    imageFile.FileName, imageFile.Length);

                // Perform Vision API analysis
                var result = await _visionService.AnalyzeImageAsync(imageFile);

                if (!result.Success)
                {
                    _logger.LogWarning("Vision analysis failed: {Error}", result.ErrorMessage);
                    return Ok(new
                    {
                        success = false,
                        message = "Analysis failed",
                        fallbackToManual = true
                    });
                }

                // Return successful analysis
                var response = new
                {
                    success = true,
                    suggestedCategoryId = result.SuggestedCategoryId,
                    categoryName = GetCategoryName(result.SuggestedCategoryId),
                    confidenceScore = result.ConfidenceScore,
                    topLabels = result.Labels.Take(5).Select(l => new
                    {
                        description = l.Description,
                        score = Math.Round(l.Score, 3)
                    }),
                    topObjects = result.Objects.Take(3).Select(o => new
                    {
                        name = o.Name,
                        score = Math.Round(o.Score, 3)
                    }),
                    processedAt = result.ProcessedAt
                };

                _logger.LogInformation("Vision analysis completed. Category: {CategoryId}, Confidence: {Confidence:P2}",
                    result.SuggestedCategoryId, result.ConfidenceScore);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during image analysis");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Analysis service temporarily unavailable",
                    fallbackToManual = true
                });
            }
        }

        /// <summary>
        /// Get analysis history for debugging/admin purposes
        /// </summary>
        /// <param name="itemId">Item ID to get analysis for</param>
        /// <returns>Analysis history</returns>
        [HttpGet("GetAnalysis/{itemId}")]
        public async Task<IActionResult> GetAnalysis(int itemId)
        {
            try
            {
                // This would require adding a method to your service to retrieve stored analyses
                // For now, return a simple response
                return Ok(new { success = true, message = "Analysis retrieval not implemented yet" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis for item {ItemId}", itemId);
                return StatusCode(500, new { success = false, message = "Error retrieving analysis" });
            }
        }

        /// <summary>
        /// Test endpoint to verify Vision API connectivity
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet("TestConnection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Create a small test image (1x1 pixel PNG)
                var testImageBytes = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");

                var result = await _visionService.AnalyzeImageAsync(testImageBytes);

                return Ok(new
                {
                    success = true,
                    visionApiWorking = result.Success,
                    message = result.Success ? "Vision API is working" : "Vision API connection failed",
                    error = result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vision API test failed");
                return Ok(new
                {
                    success = false,
                    visionApiWorking = false,
                    message = "Vision API test failed",
                    error = ex.Message
                });
            }
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
                _ => "Unknown"
            };
        }
    }
}