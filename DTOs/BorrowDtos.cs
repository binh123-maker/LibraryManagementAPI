using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class BorrowItemDto
    {
        [Required]
        public int BookId { get; set; }

        [Range(1, 10, ErrorMessage = "Số lượng mượn mỗi cuốn phải từ 1 đến 10.")]
        public int Quantity { get; set; } = 1;
    }

    public class CreateBorrowRecordDto
    {
        [Required(ErrorMessage = "Mã độc giả/sinh viên không được để trống.")]
        public int UserId { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Danh sách sách mượn không được để trống.")]
        [MinLength(1, ErrorMessage = "Phiếu mượn phải có ít nhất 1 cuốn sách.")]
        public List<BorrowItemDto> Items { get; set; } = new List<BorrowItemDto>();
    }

    public class ReturnItemDto
    {
        [Required]
        public int BorrowDetailId { get; set; }

        [StringLength(50, ErrorMessage = "Tình trạng sách tối đa 50 ký tự.")]
        public string Condition { get; set; } = "Good"; // "Good", "Damaged", "Lost"

        [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự.")]
        public string? Note { get; set; }
    }

    public class ReturnBorrowRecordDto
    {
        [StringLength(500, ErrorMessage = "Ghi chú trả sách tối đa 500 ký tự.")]
        public string? Note { get; set; }

        public List<ReturnItemDto> Items { get; set; } = new List<ReturnItemDto>();
    }

    public class ExtendBorrowRecordDto
    {
        [Range(1, 30, ErrorMessage = "Số ngày gia hạn từ 1 đến 30 ngày.")]
        public int ExtraDays { get; set; } = 7;
    }

    public class BorrowDetailResponseDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public decimal BookPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsReturned { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string BookCondition { get; set; } = "Good";
        public string? Note { get; set; }
    }

    public class BorrowRecordResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
        public string? Note { get; set; }
        public List<BorrowDetailResponseDto> Details { get; set; } = new List<BorrowDetailResponseDto>();
    }
}
