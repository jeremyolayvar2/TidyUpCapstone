namespace TidyUpCapstone.Services
{
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile imageFile);
        bool ValidateImageFile(IFormFile imageFile);
        Task<bool> DeleteImageAsync(string imageUrl);
    }

    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly string _uploadsPath;
        private readonly string[] _allowedTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public ImageService(ILogger<ImageService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _uploadsPath = Path.Combine(environment.WebRootPath, "uploads", "posts");
        }

        public async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return null;

                if (!ValidateImageFile(imageFile))
                    return null;

                // Create uploads directory if it doesn't exist
                Directory.CreateDirectory(_uploadsPath);

                // Generate unique filename
                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(_uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return relative URL
                return $"/uploads/posts/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image file");
                return null;
            }
        }

        public bool ValidateImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return false;

            // Check file type
            if (!_allowedTypes.Contains(imageFile.ContentType.ToLower()))
            {
                _logger.LogWarning("Invalid image format: {ContentType}", imageFile.ContentType);
                return false;
            }

            // Check file size
            if (imageFile.Length > MaxFileSize)
            {
                _logger.LogWarning("Image file too large: {Size} bytes", imageFile.Length);
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return true;

                // Extract filename from URL
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_uploadsPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted image file: {FileName}", fileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image file: {ImageUrl}", imageUrl);
                return false;
            }
        }
    }
}