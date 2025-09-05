using Google.Cloud.Vision.V1;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public class VisionService : IVisionService
    {
        private readonly ImageAnnotatorClient _visionClient;
        private readonly ILogger<VisionService> _logger;

        // Category mapping dictionary
        private readonly Dictionary<string, int> _categoryMappings = new()
        {
            // Books & Stationery (CategoryId: 1)
            { "book", 1 }, { "notebook", 1 }, { "pen", 1 }, { "pencil", 1 },
            { "paper", 1 }, { "magazine", 1 }, { "journal", 1 }, { "stationery", 1 },
            { "textbook", 1 }, { "novel", 1 }, { "diary", 1 },

            // Electronics & Gadgets (CategoryId: 2)
            { "smartphone", 2 }, { "phone", 2 }, { "laptop", 2 }, { "computer", 2 },
            { "tablet", 2 }, { "headphones", 2 }, { "speaker", 2 }, { "camera", 2 },
            { "television", 2 }, { "monitor", 2 }, { "keyboard", 2 }, { "mouse", 2 },
            { "gaming console", 2 }, { "smartwatch", 2 }, { "earbuds", 2 },

            // Toys & Games (CategoryId: 3)
            { "toy", 3 }, { "doll", 3 }, { "action figure", 3 }, { "board game", 3 },
            { "puzzle", 3 }, { "stuffed animal", 3 }, { "lego", 3 }, { "game", 3 },
            { "playing card", 3 }, { "ball", 3 }, { "bicycle", 3 },

            // Home & Kitchen (CategoryId: 4)
            { "plate", 4 }, { "cup", 4 }, { "bowl", 4 }, { "utensil", 4 },
            { "cookware", 4 }, { "kitchen", 4 }, { "mug", 4 }, { "glass", 4 },
            { "cutlery", 4 }, { "pot", 4 }, { "pan", 4 }, { "dish", 4 },

            // Furniture (CategoryId: 5)
            { "chair", 5 }, { "table", 5 }, { "bed", 5 }, { "sofa", 5 },
            { "desk", 5 }, { "cabinet", 5 }, { "shelf", 5 }, { "dresser", 5 },
            { "bookshelf", 5 }, { "wardrobe", 5 }, { "couch", 5 },

            // Appliances (CategoryId: 6)
            { "refrigerator", 6 }, { "microwave", 6 }, { "washing machine", 6 },
            { "dryer", 6 }, { "dishwasher", 6 }, { "oven", 6 }, { "toaster", 6 },
            { "blender", 6 }, { "coffee maker", 6 }, { "air conditioner", 6 },
            { "fan", 6 }, { "vacuum", 6 },

            // Health & Beauty (CategoryId: 7)
            { "cosmetics", 7 }, { "perfume", 7 }, { "skincare", 7 }, { "makeup", 7 },
            { "shampoo", 7 }, { "soap", 7 }, { "lotion", 7 }, { "cream", 7 },
            { "lipstick", 7 }, { "nail polish", 7 },

            // Crafts & DIY (CategoryId: 8)
            { "paint", 8 }, { "brush", 8 }, { "craft", 8 }, { "tool", 8 },
            { "hammer", 8 }, { "screwdriver", 8 }, { "drill", 8 }, { "yarn", 8 },
            { "fabric", 8 }, { "sewing", 8 },

            // School & Office (CategoryId: 9)
            { "calculator", 9 }, { "stapler", 9 }, { "printer", 9 }, { "folder", 9 },
            { "binder", 9 }, { "ruler", 9 }, { "scissors", 9 }, { "tape", 9 },
            { "backpack", 9 }, { "briefcase", 9 },

            // Sentimental Items (CategoryId: 10)
            { "photo", 10 }, { "frame", 10 }, { "jewelry", 10 }, { "watch", 10 },
            { "ring", 10 }, { "necklace", 10 }, { "bracelet", 10 }, { "trophy", 10 },
            { "medal", 10 }, { "collectible", 10 },

            // Miscellaneous (CategoryId: 11) - Default fallback
        };

        public VisionService(ILogger<VisionService> logger)
        {
            _logger = logger;

            // Initialize Vision client with service account
            var credential = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (string.IsNullOrEmpty(credential))
            {
                _logger.LogWarning("Google Vision credentials not found. Using default credentials.");
            }

            _visionClient = ImageAnnotatorClient.Create();
        }

        public async Task<VisionAnalysisResult> AnalyzeImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Image file is required");
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            return await AnalyzeImageAsync(memoryStream.ToArray());
        }

        public async Task<VisionAnalysisResult> AnalyzeImageAsync(byte[] imageBytes)
        {
            try
            {
                var image = Image.FromBytes(imageBytes);

                // Perform label detection and object localization
                var labelResponse = await _visionClient.DetectLabelsAsync(image);
                var objectResponse = await _visionClient.DetectLocalizedObjectsAsync(image);

                var result = new VisionAnalysisResult
                {
                    Labels = labelResponse.ToList(),
                    Objects = objectResponse.ToList(),
                    ProcessedAt = DateTime.UtcNow,
                    Success = true
                };

                // Determine the most likely category
                result.SuggestedCategoryId = DetermineCategoryFromAnalysis(labelResponse, objectResponse);
                result.ConfidenceScore = CalculateConfidenceScore(labelResponse, objectResponse, result.SuggestedCategoryId);

                _logger.LogInformation($"Vision analysis completed. Suggested category: {result.SuggestedCategoryId}, Confidence: {result.ConfidenceScore:P2}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image with Google Vision API");

                return new VisionAnalysisResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SuggestedCategoryId = 11, // Default to Miscellaneous
                    ConfidenceScore = 0.0m
                };
            }
        }

        private int DetermineCategoryFromAnalysis(IReadOnlyList<EntityAnnotation> labels, IReadOnlyList<LocalizedObjectAnnotation> objects)
        {
            var categoryScores = new Dictionary<int, decimal>();

            // Initialize all categories with 0 score
            for (int i = 1; i <= 11; i++)
            {
                categoryScores[i] = 0;
            }

            // Process labels
            foreach (var label in labels.Take(10)) // Top 10 labels
            {
                var labelText = label.Description.ToLower();
                var score = (decimal)label.Score;

                foreach (var mapping in _categoryMappings)
                {
                    if (labelText.Contains(mapping.Key))
                    {
                        categoryScores[mapping.Value] += score * 0.8m; // Weight labels at 80%
                        break; // Only count first match
                    }
                }
            }

            // Process objects (usually more accurate)
            foreach (var obj in objects.Take(5)) // Top 5 objects
            {
                var objectName = obj.Name.ToLower();
                var score = (decimal)obj.Score;

                foreach (var mapping in _categoryMappings)
                {
                    if (objectName.Contains(mapping.Key))
                    {
                        categoryScores[mapping.Value] += score * 1.2m; // Weight objects at 120%
                        break;
                    }
                }
            }

            // Return category with highest score, default to Miscellaneous if no matches
            var bestCategory = categoryScores.OrderByDescending(x => x.Value).First();
            return bestCategory.Value > 0 ? bestCategory.Key : 11; // 11 = Miscellaneous
        }

        private decimal CalculateConfidenceScore(IReadOnlyList<EntityAnnotation> labels, IReadOnlyList<LocalizedObjectAnnotation> objects, int suggestedCategoryId)
        {
            decimal totalConfidence = 0;
            int matchCount = 0;

            // Check confidence from labels
            foreach (var label in labels.Take(5))
            {
                var labelText = label.Description.ToLower();
                foreach (var mapping in _categoryMappings.Where(m => m.Value == suggestedCategoryId))
                {
                    if (labelText.Contains(mapping.Key))
                    {
                        totalConfidence += (decimal)label.Score;
                        matchCount++;
                        break;
                    }
                }
            }

            // Check confidence from objects
            foreach (var obj in objects.Take(3))
            {
                var objectName = obj.Name.ToLower();
                foreach (var mapping in _categoryMappings.Where(m => m.Value == suggestedCategoryId))
                {
                    if (objectName.Contains(mapping.Key))
                    {
                        totalConfidence += (decimal)obj.Score * 1.5m; // Objects are more reliable
                        matchCount++;
                        break;
                    }
                }
            }

            // Average confidence, but cap at 1.0
            return matchCount > 0 ? Math.Min(totalConfidence / matchCount, 1.0m) : 0.0m;
        }
    }

    public class VisionAnalysisResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int SuggestedCategoryId { get; set; }
        public decimal ConfidenceScore { get; set; }
        public List<EntityAnnotation> Labels { get; set; } = new();
        public List<LocalizedObjectAnnotation> Objects { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }
}