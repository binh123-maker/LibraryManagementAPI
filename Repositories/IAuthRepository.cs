using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
