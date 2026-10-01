using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class BorrowRecord
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Người mượn không được để trống.")]
        public int UserId { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Hạn trả không được để trống.")]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Trạng thái tối đa 50 ký tự.")]
        public string Status { get; set; } = "Borrowing"; // "Borrowing", "Returned", "Overdue", "Lost"

        [Column(TypeName = "decimal(18,2)")]
        public decimal FineAmount { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        public string? Note { get; set; }

        public User? User { get; set; }

        // Navigation property (1 Phiếu mượn có nhiều cuốn sách mượn chi tiết)
        public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();
    }
}