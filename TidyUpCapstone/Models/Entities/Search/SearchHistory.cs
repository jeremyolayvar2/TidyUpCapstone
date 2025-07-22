using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.Search
{
    [Table("search_history")]
    public class SearchHistory
    {
        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("search_query", TypeName = "text")]
        public string SearchQuery { get; set; } = string.Empty;

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("location_id")]
        public int? LocationId { get; set; }

        [Column("min_price", TypeName = "decimal(10,2)")]
        public decimal? MinPrice { get; set; }

        [Column("max_price", TypeName = "decimal(10,2)")]
        public decimal? MaxPrice { get; set; }

        [Column("results_count")]
        public int ResultsCount { get; set; } = 0;

        [Column("searched_at")]
        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual ItemCategory? Category { get; set; }

        [ForeignKey("LocationId")]
        public virtual ItemLocation? Location { get; set; }
    }
}