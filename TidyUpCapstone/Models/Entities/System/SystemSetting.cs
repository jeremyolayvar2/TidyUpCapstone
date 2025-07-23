using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Core;

namespace TidyUpCapstone.Models.Entities.System
{
    public class SystemSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public string SettingValue { get; set; } = string.Empty;

        [Required]
        public SettingType SettingType { get; set; } = SettingType.String;

        public string? Description { get; set; }

        public bool IsPublic { get; set; } = false;

        public int? UpdatedByAdminId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UpdatedByAdminId")]
        public virtual Admin? UpdatedByAdmin { get; set; }
    }

    public enum SettingType
    {
        String,
        Integer,
        Decimal,
        Boolean,
        Json
    }
}