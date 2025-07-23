using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;


namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class UserVisualsPurchase
    {
        [Key]
        public int UserVisualId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VisualId { get; set; }

        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;

        public bool IsEquipped { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("VisualId")]
        public virtual VisualItem VisualItem { get; set; } = null!;
    }
}