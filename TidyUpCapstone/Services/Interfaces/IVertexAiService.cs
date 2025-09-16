namespace TidyUpCapstone.Services.Interfaces
{
    public interface IVertexAiService
    {
        Task<ConditionPredictionResult> PredictConditionAsync(byte[] imageBytes);
        Task<ConditionPredictionResult> PredictConditionAsync(IFormFile imageFile);
    }
}