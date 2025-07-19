using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.SSO;


namespace TidyUpCapstone.Models.Entities.Community
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("author_id")]
        public int AuthorId { get; set; }

        [Required]
        [Column("post_content", TypeName = "text")]
        public string PostContent { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("post_type")]
        public string PostType { get; set; } = "general"; // general, achievement, item_showcase, tip

        [StringLength(255)]
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("is_pinned")]
        public bool IsPinned { get; set; } = false;

        [Column("date_posted")]
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        [Column("last_edited")]
        public DateTime? LastEdited { get; set; }

        // Navigation properties
        [ForeignKey("AuthorId")]
        public virtual AppUser Author { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}