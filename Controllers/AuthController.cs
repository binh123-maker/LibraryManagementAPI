using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // 1. Đăng ký tài khoản mới (POST /api/auth/register)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserRegisterDto request)
        {
            var result = await _userService.RegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = "Đăng ký tài khoản thành công!",
                data = result.Data
            });
        }

        // 2. Đăng nhập và nhận chuỗi JWT Token (POST /api/auth/login)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = token
            });
        }

        // 3. Đọc ngược thông tin người dùng từ Token (GET /api/auth/me)
        // Đáp ứng yêu cầu: sử dụng User.FindFirst() lấy Username & hiển thị "Xin chào [Username], bạn đang ở nhóm [Role]"
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Chưa xác định";

            return Ok(new
            {
                message = $"Xin chào {username}, bạn đang ở nhóm {role}",
                username = username,
                role = role
            });
        }
    }
}
