using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.Community;

namespace TidyUpCapstone.Models.DTOs.Community
{
    public class PostDto
    {
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorAvatarUrl { get; set; } = string.Empty;
        public string PostContent { get; set; } = string.Empty;
        public PostType PostType { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPinned { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? LastEdited { get; set; }
        public int CommentCount { get; set; }
        public int ReactionCount { get; set; }
        public ReactionType? UserReaction { get; set; }
        public List<ReactionSummaryDto> ReactionSummary { get; set; } = new List<ReactionSummaryDto>();
    }

    public class CreatePostDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string PostContent { get; set; } = string.Empty;

        public PostType PostType { get; set; } = PostType.General;

        public IFormFile? ImageFile { get; set; }
    }

    public class UpdatePostDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string PostContent { get; set; } = string.Empty;

        public PostType PostType { get; set; }
    }

    public class CommentDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public string? ParentCommenterName { get; set; }
        public int? ParentCommenterId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateCommented { get; set; }
        public DateTime? LastEdited { get; set; }
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
        public bool IsCurrentUser { get; set; }
    }

    public class CreateCommentDto
    {
        [Required]
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    public class ReactionDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public ReactionType ReactionType { get; set; }
    }

    public class ReactionSummaryDto
    {
        public ReactionType ReactionType { get; set; }
        public int Count { get; set; }
    }
}
