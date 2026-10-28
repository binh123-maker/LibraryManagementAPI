using Microsoft.AspNetCore.Http;
using WebApplication1.Models;
using static WebApplication1.DTOs.FileUploadDbDto;

namespace WebApplication1.Services
{
    public interface IFileService
    {
        Task<FileResponseDto> UploadFileAsync(IFormFile file);
        Task<FileDownloadDto?> GetFileForDownloadAsync(int id);
    }
}