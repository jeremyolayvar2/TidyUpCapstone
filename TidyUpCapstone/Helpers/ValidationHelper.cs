using TidyUpCapstone.Models.Entities.Community;

namespace TidyUpCapstone.Helpers
{
    public static class ValidationHelper
    {
        public static class PostValidation
        {
            public const int MaxContentLength = 2000;
            public const int MinContentLength = 1;

            public static (bool isValid, string? error) ValidatePostContent(string? content)
            {
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Post content is required.");

                if (content.Length > MaxContentLength)
                    return (false, $"Post content cannot exceed {MaxContentLength} characters.");

                if (content.Trim().Length < MinContentLength)
                    return (false, "Post content cannot be empty.");

                return (true, null);
            }

            public static (bool isValid, string? error) ValidatePostType(string? postTypeString, out PostType postType)
            {
                postType = PostType.General;

                if (string.IsNullOrEmpty(postTypeString))
                    return (false, "Post type is required.");

                if (!Enum.TryParse<PostType>(postTypeString, out postType))
                    return (false, "Invalid post type.");

                return (true, null);
            }
        }

        public static class CommentValidation
        {
            public const int MaxContentLength = 1000;
            public const int MinContentLength = 1;

            public static (bool isValid, string? error) ValidateCommentContent(string? content)
            {
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Comment content is required.");

                if (content.Length > MaxContentLength)
                    return (false, $"Comment content cannot exceed {MaxContentLength} characters.");

                if (content.Trim().Length < MinContentLength)
                    return (false, "Comment content cannot be empty.");

                return (true, null);
            }
        }

        public static class ImageValidation
        {
            public static readonly string[] AllowedTypes =
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            public const long MaxFileSize = 10 * 1024 * 1024; // 10MB

            public static (bool isValid, string? error) ValidateImageFile(IFormFile? imageFile)
            {
                if (imageFile == null || imageFile.Length == 0)
                    return (true, null); // Optional file

                if (!AllowedTypes.Contains(imageFile.ContentType.ToLower()))
                    return (false, "Invalid image format. Please use JPEG, PNG, GIF, or WebP.");

                if (imageFile.Length > MaxFileSize)
                    return (false, "Image file size must be less than 10MB.");

                return (true, null);
            }
        }

        public static class PaginationValidation
        {
            public const int DefaultPage = 1;
            public const int DefaultPageSize = 10;
            public const int MaxPageSize = 50;
            public const int MinPageSize = 1;

            public static (int page, int pageSize) ValidateAndNormalizePagination(int page, int pageSize)
            {
                if (page < 1) page = DefaultPage;
                if (pageSize < MinPageSize || pageSize > MaxPageSize) pageSize = DefaultPageSize;

                return (page, pageSize);
            }
        }
    }
}