using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Customization
{
    [Table("visual_items")]
    public class VisualItem
    {
        [Key]
        [Column("visual_id")]
        public int VisualId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("visual_name")]
        public string VisualName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("visual_description")]
        public string? VisualDescription { get; set; }

        [Required]
        [Column("visual_price", TypeName = "decimal(10,2)")]
        public decimal VisualPrice { get; set; } = 0.00m;

        [StringLength(255)]
        [Column("visual_img_url")]
        public string? VisualImgUrl { get; set; }

        [Required]
        [StringLength(50)]
        [Column("visual_type")]
        public string VisualType { get; set; } = string.Empty; // badge, theme, icon, border, background

        [Required]
        [StringLength(50)]
        [Column("rarity")]
        public string Rarity { get; set; } = "common"; // common, rare, epic, legendary

        [Column("is_available")]
        public bool IsAvailable { get; set; } = true;

        [Column("release_date")]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserVisualsPurchase> UserPurchases { get; set; } = new List<UserVisualsPurchase>();
    }
}