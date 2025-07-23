using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Transactions;
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

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }

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
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Cancelled,
        Disputed
    }

    public enum DeliveryMethod
    {
        Pickup,
        Courier,
        Digital
    }
}