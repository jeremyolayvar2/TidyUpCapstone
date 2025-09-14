using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Core
{
    [Table("user_privacy_settings")]
    public class UserPrivacySettings
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [MaxLength(10)]
        [Column("profile_visibility")]
        public string ProfileVisibility { get; set; } = "public";

        [MaxLength(10)]
        [Column("location_visibility")]
        public string LocationVisibility { get; set; } = "show";

        [MaxLength(10)]
        [Column("activity_streaks_visibility")]
        public string ActivityStreaksVisibility { get; set; } = "show";

        [MaxLength(10)]
        [Column("online_status")]
        public string OnlineStatus { get; set; } = "show";

        [MaxLength(10)]
        [Column("search_indexing")]
        public string SearchIndexing { get; set; } = "allow";

        [MaxLength(10)]
        [Column("contact_visibility")]
        public string ContactVisibility { get; set; } = "public";

        [MaxLength(10)]
        [Column("activity_history")]
        public string ActivityHistory { get; set; } = "show";

        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column("date_updated")]
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
    }
}