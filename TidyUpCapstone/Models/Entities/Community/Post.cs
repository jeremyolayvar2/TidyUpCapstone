using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Community
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public string PostContent { get; set; } = string.Empty;

        [Required]
        public PostType PostType { get; set; } = PostType.General;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsPinned { get; set; } = false;

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        public DateTime? LastEdited { get; set; }


        // SOFT DELETE PROPERTY
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("AuthorId")]
        public virtual AppUser Author { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }

    public enum PostType
    {
        General,
        Achievement,
        ItemShowcase,
        Tip
    }
}