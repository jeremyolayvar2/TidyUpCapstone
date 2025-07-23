using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.System
{
    public class UserLocationPreference
    {
        [Key]
        public int UserId { get; set; }

        [Column(TypeName = "decimal(10,8)")]
        public decimal? PreferredLat { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? PreferredLng { get; set; }

        public int RadiusKm { get; set; } = 10;

        public string? AddressDescription { get; set; }

        public bool AutoDetectLocation { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}
