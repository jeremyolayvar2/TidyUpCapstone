using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string directory)
        {
            try
            {
                // Validate file
                ValidateFile(file);

                var fileName = GenerateUniqueFileName(file.FileName);
                var uploadDir = Path.Combine(_environment.WebRootPath, directory);

                // Ensure directory exists
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                    _logger.LogInformation("Created directory: {Directory}", uploadDir);
                }

                var filePath = Path.Combine(uploadDir, fileName);

                // Save file
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _logger.LogInformation("File saved successfully: {FileName} to {Directory}", fileName, directory);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", file?.FileName);
                throw;
            }
        }

        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogWarning("Attempted to delete file with empty path");
                    return;
                }

                // Handle both absolute and relative paths
                var fullPath = Path.IsPathRooted(filePath)
                    ? filePath
                    : Path.Combine(_environment.WebRootPath, filePath);

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
                }
                else
                {
                    _logger.LogWarning("Attempted to delete non-existent file: {FilePath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                throw;
            }
        }

        public string GenerateUniqueFileName(string originalFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originalFileName))
                {
                    throw new ArgumentException("Original filename cannot be empty");
                }

                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var randomGuid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID

                return $"{timestamp}_{randomGuid}{extension}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating unique filename for: {OriginalFileName}", originalFileName);
                throw;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return false;

                // Check file size
                if (file.Length > _maxFileSize)
                    return false;

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    return false;

                // Check MIME type
                var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image file: {FileName}", file?.FileName);
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string fileName, string directory)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var filePath = Path.Combine(_environment.WebRootPath, directory, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return string.Empty;
            }

            return $"/{directory}/{fileName}";
        }

        public async Task<byte[]> GetFileContentAsync(string fileName, string directory)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, directory, fileName);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file content: {FileName}", fileName);
                throw;
            }
        }

        public async Task<string> SaveBase64ImageAsync(string base64Data, string directory, string? originalFileName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64Data))
                {
                    throw new ArgumentException("Base64 data cannot be empty");
                }

                // Remove data URL prefix if present
                if (base64Data.Contains(','))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                var imageBytes = Convert.FromBase64String(base64Data);

                // Generate filename
                var extension = DetermineExtensionFromBase64(base64Data) ?? ".jpg";
                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid().ToString("N")[..8]}{extension}";

                var uploadDir = Path.Combine(_environment.WebRootPath, directory);

                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var filePath = Path.Combine(uploadDir, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                _logger.LogInformation("Base64 image saved successfully: {FileName}", fileName);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving base64 image");
                throw;
            }
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty");
            }

            if (file.Length > _maxFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }

            // Additional MIME type validation
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new ArgumentException($"Invalid MIME type: {file.ContentType}");
            }
        }

        private string? DetermineExtensionFromBase64(string base64Data)
        {
            try
            {
                // Try to determine file type from base64 header
                var bytes = Convert.FromBase64String(base64Data.Substring(0, Math.Min(base64Data.Length, 100)));

                // Check for common image file signatures
                if (bytes.Length >= 4)
                {
                    // JPEG
                    if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                        return ".jpg";

                    // PNG
                    if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                        return ".png";

                    // GIF
                    if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                        return ".gif";

                    // WebP
                    if (bytes.Length >= 12 && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                        return ".webp";
                }

                return ".jpg"; // Default fallback
            }
            catch
            {
                return ".jpg"; // Default fallback
            }
        }

        public long GetMaxFileSize() => _maxFileSize;

        public string[] GetAllowedExtensions() => _allowedExtensions.ToArray();

        public async Task<bool> FileExistsAsync(string fileName, string directory)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, directory, fileName);
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

        public async Task<long> GetFileSizeAsync(string fileName, string directory)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, directory, fileName);
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    return fileInfo.Length;
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file size: {FileName}", fileName);
                return 0;
            }
        }
    }
}