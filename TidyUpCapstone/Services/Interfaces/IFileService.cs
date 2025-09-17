namespace TidyUpCapstone.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string directory);
        Task DeleteFileAsync(string filePath);
        string GenerateUniqueFileName(string originalFileName);

        // Enhanced methods for better file handling
        bool IsValidImageFile(IFormFile file);
        Task<string> GetFileUrlAsync(string fileName, string directory);
        Task<byte[]> GetFileContentAsync(string fileName, string directory);
        Task<string> SaveBase64ImageAsync(string base64Data, string directory, string? originalFileName = null);
        Task<bool> FileExistsAsync(string fileName, string directory);
        Task<long> GetFileSizeAsync(string fileName, string directory);

        // Configuration methods
        long GetMaxFileSize();
        string[] GetAllowedExtensions();
    }
}