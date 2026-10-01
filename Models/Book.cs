using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên sách tối đa 200 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả không được để trống.")]
        [StringLength(150, ErrorMessage = "Tác giả tối đa 150 ký tự.")]
        public string Author { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN tối đa 20 ký tự.")]
        public string? ISBN { get; set; }

        [StringLength(150, ErrorMessage = "Nhà xuất bản tối đa 150 ký tự.")]
        public string? Publisher { get; set; }

        public int PublishYear { get; set; }

        [Range(0, 10000, ErrorMessage = "Số lượng sách phải từ 0 đến 10,000.")]
        public int Quantity { get; set; }

        [Range(0, 10000, ErrorMessage = "Số lượng còn lại phải từ 0 đến 10,000.")]
        public int AvailableQuantity { get; set; }

        [Range(0, 100000000, ErrorMessage = "Giá sách không hợp lệ.")]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Đường dẫn ảnh bìa tối đa 500 ký tự.")]
        public string? ImageUrl { get; set; }

        [StringLength(100, ErrorMessage = "Vị trí kệ sách tối đa 100 ký tự.")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thể loại.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // Navigation property (1 Book có thể nằm trong nhiều chi tiết mượn)
        [JsonIgnore]
        public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();
    }
}