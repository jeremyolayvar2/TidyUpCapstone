using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.System
{
    public class SearchHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string SearchQuery { get; set; } = string.Empty;

        public int? CategoryId { get; set; }

        public int? LocationId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxPrice { get; set; }

        public int ResultsCount { get; set; } = 0;

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
