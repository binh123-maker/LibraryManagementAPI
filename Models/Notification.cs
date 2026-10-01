using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Tiêu đề thông báo tối đa 200 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, ErrorMessage = "Nội dung thông báo tối đa 2000 ký tự.")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Loại thông báo tối đa 50 ký tự.")]
        public string Type { get; set; } = "General"; // "DueReminder", "OverdueWarning", "LostNotice", "General"

        public bool IsSentViaEmail { get; set; } = false;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public User? User { get; set; }
    }
}
