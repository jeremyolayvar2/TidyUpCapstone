using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.DTOs
{
    public class ItemPostDto
    {
        [Required, MaxLength(100)]
        public string ItemTitle { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Condition is required")]
        public int ConditionId { get; set; }

        [Required, MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Display(Name = "Image File")]
        public IFormFile? ImageFile { get; set; }
    }
}