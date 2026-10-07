using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, string Message, UserResponseDto? Data)> RegisterAsync(UserRegisterDto request);
        Task<(bool IsSuccess, string Message, UserResponseDto? Data)> LoginAsync(LoginDto request);
        Task<(bool IsSuccess, string Message)> UpdateAsync(int id, UserRegisterDto request);
        Task<(bool IsSuccess, string Message)> DeleteAsync(int id);
    }
}
