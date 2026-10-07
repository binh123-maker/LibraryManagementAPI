using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // 1. Khai báo biến DbContext
        private readonly AppDbContext _db;

        // 2. Inject AppDbContext thông qua Constructor
        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        // ------------------------------------------------------------------------
        // READ ALL (GET)
        // GET: api/users
        // ------------------------------------------------------------------------
        [HttpGet]
        public ActionResult<List<UserResponseDto>> GetAll()
        {
            var users = _db.Users
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
                .ToList();

            return Ok(users);
        }

        // ------------------------------------------------------------------------
        // READ BY ID (GET)
        // GET: api/users/1
        // ------------------------------------------------------------------------
        [HttpGet("{id:int}")]
        public ActionResult<UserResponseDto> GetById(int id)
        {
            var user = _db.Users
                .Where(u => u.Id == id)
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
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            return Ok(user);
        }

        // ------------------------------------------------------------------------
        // CREATE / REGISTER (POST)
        // POST: api/users/register
        // ------------------------------------------------------------------------
        [HttpPost("register")]
        public ActionResult<UserResponseDto> Register([FromBody] UserRegisterDto request)
        {
            bool isExist = _db.Users.Any(u => u.Username == request.Username || u.Email == request.Email);
            if (isExist)
            {
                return BadRequest(new { message = "Username hoặc Email đã được sử dụng." });
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var newUser = new User
            {
                Username = request.Username,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Username : request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                StudentCode = request.StudentCode,
                Department = request.Department,
                ClassRoom = request.ClassRoom,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = string.IsNullOrEmpty(request.Role) ? "Reader" : request.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            var response = new UserResponseDto
            {
                Id = newUser.Id,
                FullName = newUser.FullName,
                Username = newUser.Username,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                Address = newUser.Address,
                StudentCode = newUser.StudentCode,
                Department = newUser.Department,
                ClassRoom = newUser.ClassRoom,
                Role = newUser.Role,
                CreatedAt = newUser.CreatedAt,
                IsActive = newUser.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        // ------------------------------------------------------------------------
        // LOGIN (POST)
        // POST: api/users/login
        // ------------------------------------------------------------------------
        [HttpPost("login")]
        public ActionResult<UserResponseDto> Login([FromBody] LoginDto request)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            if (!user.IsActive)
            {
                return BadRequest(new { message = "Tài khoản của bạn đã bị khóa." });
            }

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

        // ------------------------------------------------------------------------
        // UPDATE (PUT)
        // PUT: api/users/1
        // ------------------------------------------------------------------------
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UserRegisterDto request)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            // Kiểm tra trùng Email với người khác
            if (!string.IsNullOrEmpty(request.Email))
            {
                bool emailExists = _db.Users.Any(u => u.Email == request.Email && u.Id != id);
                if (emailExists)
                {
                    return BadRequest(new { message = "Email đã được sử dụng bởi người dùng khác." });
                }
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Username)) user.Username = request.Username;
            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.Address)) user.Address = request.Address;
            if (!string.IsNullOrEmpty(request.StudentCode)) user.StudentCode = request.StudentCode;
            if (!string.IsNullOrEmpty(request.Department)) user.Department = request.Department;
            if (!string.IsNullOrEmpty(request.ClassRoom)) user.ClassRoom = request.ClassRoom;
            if (!string.IsNullOrEmpty(request.Role)) user.Role = request.Role;

            if (!string.IsNullOrEmpty(request.Password))
            {
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _db.SaveChanges();

            return Ok(new { message = $"Cập nhật thành công người dùng Id = {id}" });
        }

        // ------------------------------------------------------------------------
        // DELETE
        // DELETE: api/users/1
        // ------------------------------------------------------------------------
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            _db.Users.Remove(user);
            _db.SaveChanges();

            return Ok(new { message = $"Đã xóa người dùng Id = {id}" });
        }

        // ========================================================================
        // Helper Methods: HMACSHA512 Password Hash & Verification
        // ========================================================================
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}