using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Customization;

namespace TidyUpCapstone.Models.Entities.Customization
{
    [Table("user_visuals_purchases")]
    public class UserVisualsPurchase
    {
        [Key]
        [Column("user_visual_id")]
        public int UserVisualId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("visual_id")]
        public int VisualId { get; set; }

        [Column("date_purchased")]
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;

        [Column("is_equipped")]
        public bool IsEquipped { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("VisualId")]
        public virtual VisualItem VisualItem { get; set; } = null!;
    }
}