using Google.Cloud.Vision.V1;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IVisionService
    {
        Task<VisionAnalysisResult> AnalyzeImageAsync(IFormFile imageFile);
        Task<VisionAnalysisResult> AnalyzeImageAsync(byte[] imageBytes);
    }
}