using System.ComponentModel.DataAnnotations;

namespace TidyUp.Models.Entities
{
    public class ItemLocation
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<ItemPost> ItemPosts { get; set; } = new List<ItemPost>();
    }
}