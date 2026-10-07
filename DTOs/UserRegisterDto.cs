using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên.")]
        public string Password { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        public string? PhoneNumber { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        public string? Address { get; set; }

        [StringLength(30, ErrorMessage = "Mã số sinh viên tối đa 30 ký tự.")]
        public string? StudentCode { get; set; }

        [StringLength(100, ErrorMessage = "Khoa/Viện tối đa 100 ký tự.")]
        public string? Department { get; set; }

        [StringLength(50, ErrorMessage = "Lớp sinh hoạt tối đa 50 ký tự.")]
        public string? ClassRoom { get; set; }

        [StringLength(20, ErrorMessage = "Vai trò tối đa 20 ký tự.")]
        public string? Role { get; set; }
    }
}
