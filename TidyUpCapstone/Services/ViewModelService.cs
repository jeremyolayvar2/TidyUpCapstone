using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.ViewModels.Community;
using TidyUpCapstone.Models.ViewModels.Shared;

namespace TidyUpCapstone.Services
{
    public interface IViewModelService
    {
        Task<CommunityViewModel> BuildCommunityViewModelAsync(
            List<PostDto> posts,
            List<PostDto> pinnedPosts,
            PostType? filterType,
            string? searchQuery,
            int page,
            int pageSize,
            int totalPosts,
            bool canCreatePost,
            CommunityStatsViewModel stats);

        Task<PostDetailsViewModel> BuildPostDetailsViewModelAsync(
            PostDto post,
            List<CommentDto> comments,
            List<PostDto> relatedPosts,
            bool canEdit,
            bool canDelete,
            bool canComment,
            bool canReact);
    }

    public class ViewModelService : IViewModelService
    {
        public async Task<CommunityViewModel> BuildCommunityViewModelAsync(
            List<PostDto> posts,
            List<PostDto> pinnedPosts,
            PostType? filterType,
            string? searchQuery,
            int page,
            int pageSize,
            int totalPosts,
            bool canCreatePost,
            CommunityStatsViewModel stats)
        {
            return new CommunityViewModel
            {
                Posts = posts,
                PinnedPosts = pinnedPosts,
                NewPost = new CreatePostDto(),
                Filter = new PostFilterDto
                {
                    PostType = filterType,
                    SearchQuery = searchQuery,
                    SortBy = "DatePosted",
                    SortOrder = "desc"
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalPosts
                },
                CanCreatePost = canCreatePost,
                Stats = stats
            };
        }

        public async Task<PostDetailsViewModel> BuildPostDetailsViewModelAsync(
            PostDto post,
            List<CommentDto> comments,
            List<PostDto> relatedPosts,
            bool canEdit,
            bool canDelete,
            bool canComment,
            bool canReact)
        {
            return new PostDetailsViewModel
            {
                Post = post,
                Comments = comments,
                NewComment = new CreateCommentDto { PostId = post.PostId },
                CanEdit = canEdit,
                CanDelete = canDelete,
                CanComment = canComment,
                CanReact = canReact,
                UserReaction = new ReactionDto { PostId = post.PostId },
                RelatedPosts = relatedPosts
            };
        }
    }
}