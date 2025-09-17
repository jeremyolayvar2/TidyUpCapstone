using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisionController : ControllerBase
    {
        private readonly IVisionService _visionService;
        private readonly ILogger<VisionController> _logger;
        private readonly IVertexAiService _vertexAiService;

        public VisionController(IVisionService visionService, IVertexAiService vertexAiService, ILogger<VisionController> logger)
        {
            _visionService = visionService;
            _vertexAiService = vertexAiService;
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

                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Run both Vision API (category) and Vertex AI (condition) in parallel
                var categoryTask = _visionService.AnalyzeImageAsync(imageBytes);
                var conditionTask = _vertexAiService.PredictConditionAsync(imageBytes);

                await Task.WhenAll(categoryTask, conditionTask);

                var categoryResult = await categoryTask;
                var conditionResult = await conditionTask;

                return Ok(new
                {
                    success = true,
                    // Category prediction (existing)
                    suggestedCategoryId = categoryResult.SuggestedCategoryId,
                    categoryName = GetCategoryName(categoryResult.SuggestedCategoryId),
                    confidenceScore = categoryResult.ConfidenceScore, // Keep the old name for compatibility
                    categoryConfidence = categoryResult.ConfidenceScore,
                    // Condition prediction (NEW)
                    suggestedConditionId = conditionResult.SuggestedConditionId,
                    conditionName = GetConditionName(conditionResult.SuggestedConditionId),
                    conditionConfidence = conditionResult.ConfidenceScore,
                    conditionLabel = conditionResult.PredictedLabel,
                    // Combined results
                    topLabels = categoryResult.Labels?.Take(3).Select(l => new
                    {
                        description = l.Description,
                        score = l.Score
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during image analysis");
                return BadRequest(new { success = false, message = "Analysis failed" });
            }
        }

        // Add these helper methods to your VisionController
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
                12 => "Clothing",
                _ => "Unknown Category"
            };
        }

        private string GetConditionName(int conditionId)
        {
            return conditionId switch
            {
                1 => "Excellent",
                3 => "Good",
                4 => "Fair",
                _ => "Unknown"
            };
        }

    }

}