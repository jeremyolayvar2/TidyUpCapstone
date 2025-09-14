using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Support
{
    public class ContactMessage
    {
        [Key]
        [Column("contact_id")]
        public int ContactId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [Column("category")]
        public string Category { get; set; }

        [Required]
        [StringLength(200)]
        [Column("subject")]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        [Column("message")]
        public string Message { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("is_resolved")]
        public bool IsResolved { get; set; } = false;
    }
}

