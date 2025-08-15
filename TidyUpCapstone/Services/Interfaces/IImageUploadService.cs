namespace TidyUpCapstone.Services.Interfaces
{
    public interface IImageUploadService
    {
        Task<string?> SaveImageAsync(IFormFile imageFile, string subfolder = "posts");
        Task<bool> DeleteImageAsync(string imageUrl);
        bool IsValidImageFile(IFormFile file);
        Task<string?> SaveAndResizeImageAsync(IFormFile imageFile, string subfolder = "posts", int maxWidth = 800, int maxHeight = 600);
    }
}