using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class CheckIn
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CheckInDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TokensEarned { get; set; }

        public int StreakDay { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}