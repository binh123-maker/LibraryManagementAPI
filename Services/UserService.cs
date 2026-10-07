using System.Security.Cryptography;
using System.Text;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return users.Select(MapToResponseDto).ToList();
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            return user == null ? null : MapToResponseDto(user);
        }

        public async Task<(bool IsSuccess, string Message, UserResponseDto? Data)> RegisterAsync(UserRegisterDto request)
        {
            var existingUser = await _userRepo.GetByUsernameOrEmailAsync(request.Username, request.Email);
            if (existingUser != null)
            {
                return (false, "Username hoặc Email đã được sử dụng.", null);
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

            await _userRepo.AddAsync(newUser);
            await _userRepo.SaveChangesAsync();

            return (true, "Đăng ký thành công", MapToResponseDto(newUser));
        }

        public async Task<(bool IsSuccess, string Message, UserResponseDto? Data)> LoginAsync(LoginDto request)
        {
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không chính xác.", null);
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không chính xác.", null);
            }

            if (!user.IsActive)
            {
                return (false, "Tài khoản của bạn đã bị khóa.", null);
            }

            return (true, "Đăng nhập thành công", MapToResponseDto(user));
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(int id, UserRegisterDto request)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return (false, $"Không tìm thấy người dùng có Id = {id}");
            }

            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Role)) user.Role = request.Role;
            if (!string.IsNullOrEmpty(request.Username)) user.Username = request.Username;
            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.Address)) user.Address = request.Address;
            if (!string.IsNullOrEmpty(request.StudentCode)) user.StudentCode = request.StudentCode;
            if (!string.IsNullOrEmpty(request.Department)) user.Department = request.Department;
            if (!string.IsNullOrEmpty(request.ClassRoom)) user.ClassRoom = request.ClassRoom;

            if (!string.IsNullOrEmpty(request.Password))
            {
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return (true, $"Cập nhật thành công người dùng Id = {id}");
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return (false, $"Không tìm thấy người dùng có Id = {id}");
            }

            _userRepo.Delete(user);
            await _userRepo.SaveChangesAsync();

            return (true, $"Đã xóa người dùng Id = {id}");
        }

        // Helper Private Methods
        private static UserResponseDto MapToResponseDto(User user) => new()
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

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
}
