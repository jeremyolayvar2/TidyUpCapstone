using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.Transactions
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Required]
        [Column("buyer_id")]
        public int BuyerId { get; set; }

        [Required]
        [Column("seller_id")]
        public int SellerId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("token_amount", TypeName = "decimal(10,2)")]
        public decimal TokenAmount { get; set; }

        [Required]
        [StringLength(50)]
        [Column("transaction_status")]
        public string TransactionStatus { get; set; } = "pending"; // pending, completed, cancelled, disputed

        [Required]
        [StringLength(50)]
        [Column("delivery_method")]
        public string DeliveryMethod { get; set; } = string.Empty; // pickup, courier, digital

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("cancellation_reason", TypeName = "text")]
        public string? CancellationReason { get; set; }

        // Navigation properties
        [ForeignKey("BuyerId")]
        public virtual AppUser Buyer { get; set; } = null!;

        [ForeignKey("SellerId")]
        public virtual AppUser Seller { get; set; } = null!;

        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        public virtual Chat? Chat { get; set; }
    }
}