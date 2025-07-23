using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class VisualItem
    {
        [Key]
        public int VisualId { get; set; }

        [Required]
        [StringLength(100)]
        public string VisualName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? VisualDescription { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal VisualPrice { get; set; } = 0.00m;

        [StringLength(255)]
        public string? VisualImgUrl { get; set; }

        [Required]
        public VisualType VisualType { get; set; }

        [Required]
        public VisualRarity Rarity { get; set; } = VisualRarity.Common;

        public bool IsAvailable { get; set; } = true;

        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserVisualsPurchase> UserPurchases { get; set; } = new List<UserVisualsPurchase>();
    }

    public enum VisualType
    {
        Badge,
        Theme,
        Icon,
        Border,
        Background
    }

    public enum VisualRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}