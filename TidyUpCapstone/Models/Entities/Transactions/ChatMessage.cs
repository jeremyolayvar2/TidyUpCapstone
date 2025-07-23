using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Transactions
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ChatId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public MessageType MessageType { get; set; } = MessageType.Text;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual AppUser Sender { get; set; } = null!;
    }

    public enum MessageType
    {
        Text,
        System,
        Image,
        Location
    }
}