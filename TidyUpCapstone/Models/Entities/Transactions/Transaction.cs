// Models/Entities/Transactions/Transaction.cs - Enhanced version
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Transactions
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TokenAmount { get; set; }

        [Required]
        public TransactionStatus TransactionStatus { get; set; } = TransactionStatus.Pending;

        // Original delivery method (keep for compatibility)
        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }

        // NEW: Confirmation tracking
        public bool BuyerConfirmed { get; set; } = false;
        public bool SellerConfirmed { get; set; } = false;
        public DateTime? BuyerConfirmedAt { get; set; }
        public DateTime? SellerConfirmedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

        // Navigation properties
        [ForeignKey("BuyerId")]
        public virtual AppUser Buyer { get; set; } = null!;

        [ForeignKey("SellerId")]
        public virtual AppUser Seller { get; set; } = null!;

        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        public virtual Chat? Chat { get; set; }
        public virtual ICollection<Escrow> Escrows { get; set; } = new List<Escrow>();
    }

    // Enhanced transaction status
    public enum TransactionStatus
    {
        Pending = 0,
        Escrowed = 1,
        Confirmed = 2,
        Cancelled = 3
    }

    // Keep existing DeliveryMethod for compatibility
    public enum DeliveryMethod
    {
        Pickup,
        Courier,
        Digital
    }
}