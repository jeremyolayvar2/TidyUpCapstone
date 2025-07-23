using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.Entities.Items
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int ConditionId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemTitle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AdjustedTokenPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FinalTokenPrice { get; set; }

        [StringLength(255)]
        public string? ImageFileName { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? AiSuggestedPrice { get; set; }

        public bool PriceOverriddenByUser { get; set; } = false;

        [Required]
        public AiProcessingStatus AiProcessingStatus { get; set; } = AiProcessingStatus.Pending;

        public DateTime? AiProcessedAt { get; set; }

        [StringLength(100)]
        public string? AiDetectedCategory { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AiConditionScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AiConfidenceLevel { get; set; }

        [Required]
        public ItemStatus Status { get; set; } = ItemStatus.Available;

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public int ViewCount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual ItemCategory Category { get; set; } = null!;

        [ForeignKey("ConditionId")]
        public virtual ItemCondition Condition { get; set; } = null!;

        [ForeignKey("LocationId")]
        public virtual ItemLocation Location { get; set; } = null!;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<AzureCvAnalysis> AzureCvAnalyses { get; set; } = new List<AzureCvAnalysis>();
        public virtual ICollection<TensorflowPrediction> TensorflowPredictions { get; set; } = new List<TensorflowPrediction>();
        public virtual ICollection<AiProcessingPipeline> AiProcessingPipelines { get; set; } = new List<AiProcessingPipeline>();
        public virtual ICollection<AiTrainingFeedback> AiTrainingFeedbacks { get; set; } = new List<AiTrainingFeedback>();
    }

    public enum AiProcessingStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    public enum ItemStatus
    {
        Available,
        Claimed,
        Completed,
        Removed
    }
}