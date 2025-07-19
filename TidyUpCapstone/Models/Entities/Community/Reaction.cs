using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Community;


namespace TidyUpCapstone.Models.Entities.Community
{
    [Table("reactions")]
    public class Reaction
    {
        [Key]
        [Column("reaction_id")]
        public int ReactionId { get; set; }

        [Required]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("reaction_type")]
        public string ReactionType { get; set; } = "like"; // like, love, helpful, inspiring

        [Column("date_reacted")]
        public DateTime DateReacted { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}