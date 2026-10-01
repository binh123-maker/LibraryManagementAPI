using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class BorrowDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BorrowRecordId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng mượn tối thiểu là 1.")]
        public int Quantity { get; set; } = 1;

        public bool IsReturned { get; set; } = false;

        public DateTime? ReturnedDate { get; set; }

        [StringLength(50, ErrorMessage = "Tình trạng sách tối đa 50 ký tự.")]
        public string BookCondition { get; set; } = "Good"; // "Good", "Damaged", "Lost"

        [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự.")]
        public string? Note { get; set; }

        [JsonIgnore]
        public BorrowRecord? BorrowRecord { get; set; }

        public Book? Book { get; set; }
    }
}