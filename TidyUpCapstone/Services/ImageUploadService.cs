using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly ILogger<ImageUploadService> _logger;
        private readonly IWebHostEnvironment _environment;

        // Allowed file types and max file size
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public ImageUploadService(ILogger<ImageUploadService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(IFormFile imageFile, string subfolder = "posts")
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    _logger.LogWarning("No image file provided");
                    return null;
                }

                // Validate file
                if (!IsValidImageFile(imageFile))
                {
                    return null;
                }

                // Create uploads directory
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return relative URL
                var imageUrl = $"/uploads/{subfolder}/{fileName}";
                _logger.LogInformation("Image saved successfully: {ImageUrl}", imageUrl);

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image file: {FileName}", imageFile.FileName);
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Convert URL to file path
                var relativePath = imageUrl.TrimStart('/');
                var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Image deleted successfully: {ImageUrl}", imageUrl);
                    return true;
                }

                _logger.LogWarning("Image file not found: {ImageUrl}", imageUrl);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file: null or empty");
                return false;
            }

            // Check file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Invalid file type: {Extension}", fileExtension);
                return false;
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("File too large: {Size} bytes", file.Length);
                return false;
            }

            // Check content type
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Invalid content type: {ContentType}", file.ContentType);
                return false;
            }

            return true;
        }

        public async Task<string?> SaveAndResizeImageAsync(IFormFile imageFile, string subfolder = "posts", int maxWidth = 800, int maxHeight = 600)
        {
            // TODO: Implement image resizing using ImageSharp or similar library
            // For now, just save the original image
            return await SaveImageAsync(imageFile, subfolder);
        }
    }
}