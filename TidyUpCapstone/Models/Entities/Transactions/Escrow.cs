using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Transactions
{
    public class Escrow
    {
        [Key]
        public int EscrowId { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public EscrowStatus Status { get; set; } = EscrowStatus.Held;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; } = null!;
    }

    
}