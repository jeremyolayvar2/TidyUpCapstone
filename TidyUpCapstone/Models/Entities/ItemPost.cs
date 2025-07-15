using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TidyUpCapstone.Models.Entities
{
    public class ItemPost
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string ItemTitle { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual ItemCategory Category { get; set; } = null!;

        [Required]
        public int ConditionId { get; set; }

        [ForeignKey(nameof(ConditionId))]
        public virtual ItemCondition Condition { get; set; } = null!;

        [Required]
        public int LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public virtual ItemLocation Location { get; set; } = null!;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Precision(10, 2)]
        public decimal AdjustedTokenPrice { get; set; }

        [Precision(10, 2)]
        public decimal FinalTokenPrice { get; set; }

        [MaxLength(255)]
        public string ImageFileName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}