using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.DTOs.Transactions
{
    public class TransactionConfirmationDto
    {
        [Required]
        public int TransactionId { get; set; }
        public bool Confirm { get; set; } = true;
    }

    public class TransactionCancellationDto
    {
        [Required]
        public int TransactionId { get; set; }
        [StringLength(500)]
        public string? Reason { get; set; }
    }

    public class TransactionStatusDto
    {
        public int TransactionId { get; set; }
        public TransactionStatus Status { get; set; }
        public bool BuyerConfirmed { get; set; }
        public bool SellerConfirmed { get; set; }
        public bool CanConfirm { get; set; }
        public bool CanCancel { get; set; }
        public string UserRole { get; set; } = string.Empty;
    }
}