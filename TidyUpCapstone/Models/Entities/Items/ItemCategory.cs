using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Search;

namespace TidyUpCapstone.Models.Entities.Items
{
    [Table("item_category")]
    public class ItemCategory
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}