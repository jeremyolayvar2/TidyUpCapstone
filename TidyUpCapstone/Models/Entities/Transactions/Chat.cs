using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.Entities.Transactions
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        [Required]
        public int TransactionId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime LastMessageTime { get; set; } = DateTime.UtcNow;

        public bool EscrowReleased { get; set; } = false;

        public bool BuyerConfirmed { get; set; } = false;

        public bool SellerConfirmed { get; set; } = false;

        public DateTime? DateClaimed { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EscrowAmount { get; set; }

        public DateTime? CompletionDate { get; set; }

        [Required]
        public EscrowStatus EscrowStatus { get; set; } = EscrowStatus.Pending;

        // Navigation properties
        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; } = null!;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public enum EscrowStatus
    {
        Pending,
        Released,
        Disputed,
        Refunded
    }
}