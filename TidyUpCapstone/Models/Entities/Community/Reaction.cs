using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Community
{
    public class Reaction
    {
        [Key]
        public int ReactionId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public ReactionType ReactionType { get; set; } = ReactionType.Like;

        public DateTime DateReacted { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }

    public enum ReactionType
    {
        Like,
        Love,
        Helpful,
        Inspiring
    }
}