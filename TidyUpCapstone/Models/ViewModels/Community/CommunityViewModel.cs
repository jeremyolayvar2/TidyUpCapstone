using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.ViewModels.Shared;

namespace TidyUpCapstone.Models.ViewModels.Community
{
    public class CommunityViewModel
    {
        public List<PostDto> Posts { get; set; } = new List<PostDto>();
        public List<PostDto> PinnedPosts { get; set; } = new List<PostDto>();
        public CreatePostDto NewPost { get; set; } = new CreatePostDto();
        public PostFilterDto Filter { get; set; } = new PostFilterDto();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
        public bool CanCreatePost { get; set; }
        public List<string> TrendingTopics { get; set; } = new List<string>();
        public CommunityStatsViewModel Stats { get; set; } = new CommunityStatsViewModel();
    }

    public class PostDetailsViewModel
    {
        public PostDto Post { get; set; } = null!;
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public CreateCommentDto NewComment { get; set; } = new CreateCommentDto();
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanComment { get; set; }
        public bool CanReact { get; set; }
        public ReactionDto UserReaction { get; set; } = new ReactionDto();
        public List<PostDto> RelatedPosts { get; set; } = new List<PostDto>();
    }

    public class PostFilterDto
    {
        public PostType? PostType { get; set; }
        public string? SearchQuery { get; set; }
        public string? AuthorUsername { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; } = "DatePosted";
        public string? SortOrder { get; set; } = "desc";
        public bool ShowPinnedOnly { get; set; } = false;
    }

    public class CommunityStatsViewModel
    {
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalReactions { get; set; }
        public int ActiveUsers { get; set; }
        public List<string> MostActiveUsers { get; set; } = new List<string>();
        public List<PostDto> TrendingPosts { get; set; } = new List<PostDto>();
    }
}