using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        public string? PhoneNumber { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Vai trò không được để trống.")]
        [StringLength(20, ErrorMessage = "Vai trò tối đa 20 ký tự.")]
        public string Role { get; set; } = "Reader";

        public bool IsActive { get; set; } = true;
    }
}