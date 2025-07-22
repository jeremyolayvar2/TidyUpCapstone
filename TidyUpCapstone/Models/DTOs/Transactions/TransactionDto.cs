using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.DTOs.Transactions
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public int BuyerId { get; set; }
        public string BuyerUsername { get; set; } = string.Empty;
        public string BuyerAvatarUrl { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public string SellerUsername { get; set; } = string.Empty;
        public string SellerAvatarUrl { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string ItemImageUrl { get; set; } = string.Empty;
        public decimal TokenAmount { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public ChatDto? Chat { get; set; }
    }

    public class CreateTransactionDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }
    }

    public class UpdateTransactionStatusDto
    {
        [Required]
        public TransactionStatus Status { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }

    public class ChatDto
    {
        public int ChatId { get; set; }
        public int TransactionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastMessageTime { get; set; }
        public bool EscrowReleased { get; set; }
        public bool BuyerConfirmed { get; set; }
        public bool SellerConfirmed { get; set; }
        public decimal EscrowAmount { get; set; }
        public EscrowStatus EscrowStatus { get; set; }
        public List<ChatMessageDto> RecentMessages { get; set; } = new List<ChatMessageDto>();
        public int UnreadCount { get; set; }
    }

    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderAvatarUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class SendMessageDto
    {
        [Required]
        public int ChatId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        public MessageType MessageType { get; set; } = MessageType.Text;
    }
}