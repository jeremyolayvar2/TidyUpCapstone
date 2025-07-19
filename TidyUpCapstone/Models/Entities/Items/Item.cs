using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.Entities.Items
{
    [Table("items")]
    public class Item
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("condition_id")]
        public int ConditionId { get; set; }

        [Required]
        [Column("location_id")]
        public int LocationId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("item_title")]
        public string ItemTitle { get; set; } = string.Empty;

        [Column("latitude", TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }

        [Column("longitude", TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("adjusted_token_price", TypeName = "decimal(10,2)")]
        public decimal AdjustedTokenPrice { get; set; }

        [Required]
        [Column("final_token_price", TypeName = "decimal(10,2)")]
        public decimal FinalTokenPrice { get; set; }

        [StringLength(255)]
        [Column("image_file_name")]
        public string? ImageFileName { get; set; }

        [Column("ai_suggested_price", TypeName = "decimal(10,2)")]
        public decimal? AiSuggestedPrice { get; set; }

        [Column("price_overriden_by_user")]
        public bool PriceOverriddenByUser { get; set; } = false;

        [Required]
        [StringLength(50)]
        [Column("ai_processing_status")]
        public string AiProcessingStatus { get; set; } = "pending"; // pending, processing, completed, failed

        [Column("ai_processed_at")]
        public DateTime? AiProcessedAt { get; set; }

        [StringLength(100)]
        [Column("ai_detected_category")]
        public string? AiDetectedCategory { get; set; }

        [Column("ai_condition_score", TypeName = "decimal(5,2)")]
        public decimal? AiConditionScore { get; set; }

        [Column("ai_confidence_level", TypeName = "decimal(5,2)")]
        public decimal? AiConfidenceLevel { get; set; }

        [Required]
        [StringLength(50)]
        [Column("status")]
        public string Status { get; set; } = "available"; // available, claimed, completed, removed

        [Column("date_posted")]
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("view_count")]
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
}