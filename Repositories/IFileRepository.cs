using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IFileRepository
    {
        Task<StoredFile> AddAsync(StoredFile file);
        Task<StoredFile?> GetByIdAsync(int id);
    }
}
