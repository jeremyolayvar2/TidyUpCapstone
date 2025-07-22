using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;

namespace TidyUpCapstone.Models.Entities.Search
{
    [Table("user_location_preferences")]
    public class UserLocationPreference
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("preferred_lat", TypeName = "decimal(10,8)")]
        public decimal? PreferredLat { get; set; }

        [Column("preferred_lng", TypeName = "decimal(11,8)")]
        public decimal? PreferredLng { get; set; }

        [Column("radius_km")]
        public int RadiusKm { get; set; } = 10;

        [Column("address_description", TypeName = "text")]
        public string? AddressDescription { get; set; }

        [Column("auto_detect_location")]
        public bool AutoDetectLocation { get; set; } = true;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}