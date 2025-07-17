using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities
{
    [Table("Message")]
    public class Messages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("Text")]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Foreign Keys

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public virtual ApplicationUser Sender { get; set; } = null!;

        [Required]
        public string BuyerId { get; set; } = string.Empty;

        [ForeignKey(nameof(BuyerId))]
        public virtual ApplicationUser Buyer { get; set; } = null!;

        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey(nameof(SellerId))]
        public virtual ApplicationUser Seller { get; set; } = null!;

        [Required]
        public int ItemPostId { get; set; }

        [ForeignKey(nameof(ItemPostId))]
        public virtual ItemPost ItemPost { get; set; } = null!;
    }
}
