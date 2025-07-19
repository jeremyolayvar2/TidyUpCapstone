using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.Items
{
    [Table("itemcondition")]
    public class ItemCondition
    {
        [Key]
        [Column("condition_id")]
        public int ConditionId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("condition_multiplier", TypeName = "decimal(3,2)")]
        public decimal ConditionMultiplier { get; set; } = 1.00m;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}