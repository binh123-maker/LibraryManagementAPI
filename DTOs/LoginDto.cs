using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; } = string.Empty;
    }
}