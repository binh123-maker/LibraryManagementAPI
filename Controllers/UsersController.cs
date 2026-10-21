using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // Inject duy nhất IUserService để quản lý tài nguyên User
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. Chỉ Admin mới có quyền xem danh sách tất cả người dùng
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserResponseDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // 2. Cả Admin và Student/User/Reader đều có quyền xem thông tin chi tiết người dùng
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Student,User,Reader")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            return Ok(user);
        }

        // 3. Cập nhật thông tin người dùng
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Student,User,Reader")]
        public async Task<IActionResult> Update(int id, [FromBody] UserRegisterDto request)
        {
            var result = await _userService.UpdateAsync(id, request);
            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // 4. Xoá người dùng: Chỉ Admin mới có quyền
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // ====== Endpoint test phân quyền ======

        // Chỉ Token có Role = "Admin" mới gọi được
        [HttpGet("test-admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult TestAdminOnly()
        {
            return Ok(new
            {
                message = "Xác thực thành công! Bạn đang truy cập với quyền ADMIN.",
                username = User.Identity?.Name,
                role = "Admin"
            });
        }

        // Token có Role là "Student", "User" hoặc "Reader" đều gọi được
        [HttpGet("test-student")]
        [Authorize(Roles = "Student,User,Reader")]
        public IActionResult TestStudentOnly()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            return Ok(new
            {
                message = $"Xác thực thành công! Bạn đang truy cập với quyền {role}.",
                username = User.Identity?.Name,
                role = role
            });
        }
    }
}