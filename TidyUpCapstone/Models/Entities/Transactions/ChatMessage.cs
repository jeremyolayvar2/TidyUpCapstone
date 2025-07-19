using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Transactions;


namespace TidyUpCapstone.Models.Entities.Transactions
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Key]
        [Column("message_id")]
        public int MessageId { get; set; }

        [Required]
        [Column("chat_id")]
        public int ChatId { get; set; }

        [Required]
        [Column("sender_id")]
        public int SenderId { get; set; }

        [Required]
        [Column("message", TypeName = "text")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("message_type")]
        public string MessageType { get; set; } = "text"; // text, system, image, location

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual AppUser Sender { get; set; } = null!;
    }
}