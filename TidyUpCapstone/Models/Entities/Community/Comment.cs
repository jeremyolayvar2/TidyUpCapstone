using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Community;

namespace TidyUpCapstone.Models.Entities.Community
{
    [Table("comments")]
    public class Comment
    {
        [Key]
        [Column("comment_id")]
        public int CommentId { get; set; }

        [Required]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("parent_comment_id")]
        public int? ParentCommentId { get; set; }

        [Required]
        [Column("content", TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Column("date_commented")]
        public DateTime DateCommented { get; set; } = DateTime.UtcNow;

        [Column("last_edited")]
        public DateTime? LastEdited { get; set; }

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment> ChildComments { get; set; } = new List<Comment>();
    }
}