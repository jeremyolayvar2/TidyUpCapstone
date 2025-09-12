using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using TidyUpCapstone.Services.Interfaces;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace TidyUpCapstone.Services
{
    public class VertexAiService : IVertexAiService
    {
        private readonly PredictionServiceClient _client;
        private readonly ILogger<VertexAiService> _logger;
        private readonly string _projectId;
        private readonly string _location;
        private readonly string _endpointId;

        public VertexAiService(ILogger<VertexAiService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _projectId = configuration["GoogleCloud:ProjectId"] ?? "amazing-limiter-469803-h6";
            _location = configuration["VertexAI:Location"] ?? "us-central1";
            _endpointId = configuration["VertexAI:EndpointId"] ?? "8266294443875762176";

            _client = PredictionServiceClient.Create();
        }

        public async Task<ConditionPredictionResult> PredictConditionAsync(byte[] imageBytes)
        {
            try
            {
                var imageBase64 = Convert.ToBase64String(imageBytes);

                // Construct the endpoint name
                var endpointName = $"projects/{_projectId}/locations/{_location}/endpoints/{_endpointId}";

                var instance = Value.ForStruct(new Google.Protobuf.WellKnownTypes.Struct
                {
                    Fields =
                    {
                        ["content"] = Value.ForString(imageBase64),
                        ["mimeType"] = Value.ForString("image/jpeg")
                    }
                });

                var request = new PredictRequest
                {
                    Endpoint = endpointName,
                    Instances = { instance }
                };

                _logger.LogInformation($"Making Vertex AI prediction request to: {endpointName}");

                var response = await _client.PredictAsync(request);
                return ProcessPredictionResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting item condition with Vertex AI");

                return new ConditionPredictionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SuggestedConditionId = 3, // Default to Good
                    ConfidenceScore = 0.0m
                };
            }
        }

        public async Task<ConditionPredictionResult> PredictConditionAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Image file is required");
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            return await PredictConditionAsync(memoryStream.ToArray());
        }

        private ConditionPredictionResult ProcessPredictionResponse(PredictResponse response)
        {
            if (!response.Predictions.Any())
            {
                return new ConditionPredictionResult
                {
                    Success = false,
                    ErrorMessage = "No predictions returned",
                    SuggestedConditionId = 3,
                    ConfidenceScore = 0.0m
                };
            }

            var prediction = response.Predictions.First();

            _logger.LogInformation($"Raw Vertex AI response fields: {string.Join(", ", prediction.StructValue.Fields.Keys)}");
            _logger.LogInformation($"Full response: {prediction.StructValue}");
            // Handle both possible response formats
            var confidences = new List<double>();
            var displayNames = new List<string>();

            try
            {
                // Try format 1: confidences and displayNames arrays
                if (prediction.StructValue.Fields.ContainsKey("confidences") &&
                    prediction.StructValue.Fields.ContainsKey("displayNames"))
                {
                    confidences = prediction.StructValue.Fields["confidences"]
                        .ListValue.Values.Select(v => v.NumberValue).ToList();
                    displayNames = prediction.StructValue.Fields["displayNames"]
                        .ListValue.Values.Select(v => v.StringValue).ToList();
                }
                // Try format 2: direct label-confidence pairs
                else if (prediction.StructValue.Fields.ContainsKey("predictions"))
                {
                    var predictions = prediction.StructValue.Fields["predictions"].ListValue.Values;
                    foreach (var pred in predictions)
                    {
                        var predStruct = pred.StructValue;
                        if (predStruct.Fields.ContainsKey("label") && predStruct.Fields.ContainsKey("confidence"))
                        {
                            displayNames.Add(predStruct.Fields["label"].StringValue);
                            confidences.Add(predStruct.Fields["confidence"].NumberValue);
                        }
                    }
                }
                else
                {
                    // Fallback: try to extract any confidence scores
                    _logger.LogWarning("Unknown prediction response format. Available fields: {Fields}",
                        string.Join(", ", prediction.StructValue.Fields.Keys));

                    // Return default result
                    return new ConditionPredictionResult
                    {
                        Success = false,
                        ErrorMessage = "Unknown response format from Vertex AI",
                        SuggestedConditionId = 3,
                        ConfidenceScore = 0.0m
                    };
                }

                if (!confidences.Any() || !displayNames.Any())
                {
                    return new ConditionPredictionResult
                    {
                        Success = false,
                        ErrorMessage = "No predictions found in response",
                        SuggestedConditionId = 3,
                        ConfidenceScore = 0.0m
                    };
                }

                // Find the prediction with highest confidence
                var bestPrediction = confidences
                    .Select((conf, index) => new {
                        Confidence = conf,
                        Label = displayNames[index],
                        Index = index
                    })
                    .OrderByDescending(p => p.Confidence)
                    .First();

                var conditionId = MapVertexLabelToConditionId(bestPrediction.Label);

                _logger.LogInformation($"Vertex AI prediction: {bestPrediction.Label} with confidence {bestPrediction.Confidence:P2}, mapped to condition ID {conditionId}");

                return new ConditionPredictionResult
                {
                    Success = true,
                    SuggestedConditionId = conditionId,
                    ConfidenceScore = (decimal)bestPrediction.Confidence,
                    PredictedLabel = bestPrediction.Label,
                    AllPredictions = confidences
                        .Select((conf, index) => new ConditionPrediction
                        {
                            Label = displayNames[index],
                            Confidence = (decimal)conf
                        })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Vertex AI response");
                return new ConditionPredictionResult
                {
                    Success = false,
                    ErrorMessage = $"Error processing response: {ex.Message}",
                    SuggestedConditionId = 3,
                    ConfidenceScore = 0.0m
                };
            }
        }

        private int MapVertexLabelToConditionId(string vertexLabel)
        {
            return vertexLabel?.ToLower() switch
            {
                "new_excellent" => 1, // Excellent
                "good" => 3,          // Good  
                "fair" => 4,          // Fair
                _ => 3                // Default to Good
            };
        }
    }

    // Data classes for Vertex AI results
    public class ConditionPredictionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int SuggestedConditionId { get; set; }
        public decimal ConfidenceScore { get; set; }
        public string? PredictedLabel { get; set; }
        public List<ConditionPrediction> AllPredictions { get; set; } = new();
    }

    public class ConditionPrediction
    {
        public string Label { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
    }
}