using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        public string? PhoneNumber { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Vai trò không được để trống.")]
        [StringLength(20, ErrorMessage = "Vai trò tối đa 20 ký tự.")]
        public string Role { get; set; } = "Reader";

        [StringLength(30, ErrorMessage = "Mã số sinh viên tối đa 30 ký tự.")]
        public string? StudentCode { get; set; }

        [StringLength(100, ErrorMessage = "Khoa/Viện tối đa 100 ký tự.")]
        public string? Department { get; set; }

        [StringLength(50, ErrorMessage = "Lớp sinh hoạt tối đa 50 ký tự.")]
        public string? ClassRoom { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property (1 User có nhiều phiếu mượn)
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        // Navigation property (1 User có nhiều thông báo)
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}