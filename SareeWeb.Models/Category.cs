using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SareeWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Range should be in between 1 and 100")]
        public int DisplayOrder { get; set; }
        public DateTime CreationDateTime { get; set; } = DateTime.Now;
    }
}
