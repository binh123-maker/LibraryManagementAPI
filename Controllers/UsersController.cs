using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // 1. GET ALL USERS + SEARCH BY NAME
        //
        // GET: api/users
        // GET: api/users?name=Nguyen
        // =====================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers(
            [FromQuery] string? name)
        {
            // Tạo query từ bảng Users
            var query = _context.Users.AsQueryable();


            // Nếu có nhập tên thì tìm theo FullName
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(u => u.FullName.Contains(name));
            }


            // Lấy dữ liệu và chuyển sang UserResponseDto
            var users = await query
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    StudentCode = u.StudentCode,
                    Department = u.Department,
                    ClassRoom = u.ClassRoom,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();


            // Nếu có tìm kiếm theo Name nhưng không tìm thấy
            if (!string.IsNullOrWhiteSpace(name) && users.Count == 0)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy người dùng phù hợp."
                });
            }


            return Ok(users);
        }


        // =====================================================
        // 2. CREATE USER
        // POST: api/users
        // =====================================================

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(
            [FromBody] RegisterDto dto)
        {
            // Kiểm tra Username đã tồn tại chưa
            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);

            if (usernameExists)
            {
                return BadRequest(new
                {
                    message = "Tên đăng nhập đã tồn tại."
                });
            }


            // Kiểm tra Email đã tồn tại chưa
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "Email đã tồn tại."
                });
            }


            // Tạo User mới
            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                StudentCode = dto.StudentCode,
                Department = dto.Department,
                ClassRoom = dto.ClassRoom,

                Role = "Reader",

                CreatedAt = DateTime.UtcNow,

                IsActive = true
            };


            // Hash mật khẩu
            var passwordHasher = new PasswordHasher<User>();

            user.PasswordHash = passwordHasher.HashPassword(
                user,
                dto.Password
            );


            // Thêm vào database
            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            // Dữ liệu trả về
            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                StudentCode = user.StudentCode,
                Department = user.Department,
                ClassRoom = user.ClassRoom,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };


            // Trả HTTP 201 Created
            return CreatedAtAction(
                nameof(GetUsers),
                new { id = user.Id },
                response
            );
        }


        // =====================================================
        // 3. UPDATE USER
        // PUT: api/users/1
        // =====================================================

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(
            int id,
            [FromBody] UserUpdateDto dto)
        {
            // Tìm User cần cập nhật
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy người dùng có Id = {id}"
                });
            }


            // Kiểm tra Email có bị trùng với User khác không
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "Email đã được sử dụng bởi người dùng khác."
                });
            }


            // Cập nhật thông tin
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;
            user.StudentCode = dto.StudentCode;
            user.Department = dto.Department;
            user.ClassRoom = dto.ClassRoom;
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;


            // Lưu database
            await _context.SaveChangesAsync();


            // Dữ liệu trả về
            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                StudentCode = user.StudentCode,
                Department = user.Department,
                ClassRoom = user.ClassRoom,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };


            return Ok(response);
        }


        // =====================================================
        // 4. DELETE USER
        // DELETE: api/users/1
        // =====================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Tìm User cần xóa
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy người dùng có Id = {id}"
                });
            }


            // Xóa User
            _context.Users.Remove(user);


            // Lưu database
            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Xóa người dùng thành công."
            });
        }
    }
}