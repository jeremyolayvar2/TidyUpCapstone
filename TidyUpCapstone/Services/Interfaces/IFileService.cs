namespace TidyUpCapstone.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string directory);
        Task DeleteFileAsync(string filePath);
        string GenerateUniqueFileName(string originalFileName);
    }
}