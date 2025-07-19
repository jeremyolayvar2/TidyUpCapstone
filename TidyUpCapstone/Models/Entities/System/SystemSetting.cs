using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;

namespace TidyUpCapstone.Models.Entities.System
{
    [Table("system_settings")]
    public class SystemSetting
    {
        [Key]
        [Column("setting_id")]
        public int SettingId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("setting_key")]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [Column("setting_value", TypeName = "text")]
        public string SettingValue { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("setting_type")]
        public string SettingType { get; set; } = "string"; // string, integer, decimal, boolean, json

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("is_public")]
        public bool IsPublic { get; set; } = false;

        [Column("updated_by_admin_id")]
        public int? UpdatedByAdminId { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UpdatedByAdminId")]
        public virtual Admin? UpdatedByAdmin { get; set; }
    }
}