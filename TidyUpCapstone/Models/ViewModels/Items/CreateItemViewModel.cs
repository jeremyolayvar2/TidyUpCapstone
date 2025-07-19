using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TidyUpCapstone.Models.ViewModels.Items
{
    public class CreateItemViewModel
    {
        [Required(ErrorMessage = "Item title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        [Display(Name = "Item Title")]
        public string ItemTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please select a condition")]
        [Display(Name = "Condition")]
        public int ConditionId { get; set; }

        [Required(ErrorMessage = "Please select a location")]
        [Display(Name = "Location")]
        public int LocationId { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99")]
        [Display(Name = "Token Price (Optional - AI will suggest if not provided)")]
        public decimal? UserSetPrice { get; set; }

        [Display(Name = "Item Image")]
        public IFormFile? ItemImage { get; set; }

        // Location coordinates (optional for precise location)
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        [Display(Name = "Expires At (Optional)")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAt { get; set; }

        // Available options for dropdowns
        public List<CategoryOption> Categories { get; set; } = new List<CategoryOption>();
        public List<ConditionOption> Conditions { get; set; } = new List<ConditionOption>();
        public List<LocationOption> Locations { get; set; } = new List<LocationOption>();
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ConditionOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Multiplier { get; set; }
    }

    public class LocationOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}