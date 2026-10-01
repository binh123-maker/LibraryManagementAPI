using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thể loại không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên thể loại tối đa 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự.")]
        public string? Description { get; set; }

        // Navigation property (1 Category có nhiều Book)
        [JsonIgnore]
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}