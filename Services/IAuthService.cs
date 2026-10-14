namespace WebApplication1.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(DTOs.LoginDto dto);
    }
}
