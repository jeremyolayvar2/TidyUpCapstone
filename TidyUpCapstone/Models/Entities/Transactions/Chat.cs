using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Transactions
{
    [Table("chats")]
    public class Chat
    {
        [Key]
        [Column("chat_id")]
        public int ChatId { get; set; }

        [Required]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [Column("last_message_time")]
        public DateTime LastMessageTime { get; set; } = DateTime.UtcNow;

        [Column("escrow_released")]
        public bool EscrowReleased { get; set; } = false;

        [Column("buyer_confirmed")]
        public bool BuyerConfirmed { get; set; } = false;

        [Column("seller_confirmed")]
        public bool SellerConfirmed { get; set; } = false;

        [Column("date_claimed")]
        public DateTime? DateClaimed { get; set; }

        [Required]
        [Column("escrow_amount", TypeName = "decimal(10,2)")]
        public decimal EscrowAmount { get; set; }

        [Column("completion_date")]
        public DateTime? CompletionDate { get; set; }

        [Required]
        [StringLength(50)]
        [Column("escrow_status")]
        public string EscrowStatus { get; set; } = "pending"; // pending, released, disputed, refunded

        // Navigation properties
        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; } = null!;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}