using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Search;

namespace TidyUpCapstone.Models.Entities.Items
{
    [Table("itemlocation")]
    public class ItemLocation
    {
        [Key]
        [Column("location_id")]
        public int LocationId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("region")]
        public string? Region { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}