using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Cấu hình kiểu dữ liệu chính xác cho Price
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }

        // Thêm trường Description cho phép null
        public string? Description { get; set; }

    }
}
