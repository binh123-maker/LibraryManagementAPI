using Microsoft.AspNetCore.Http;
using WebApplication1.Models;
using WebApplication1.Repositories;
using static WebApplication1.DTOs.FileUploadDbDto;

namespace WebApplication1.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;

        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx", ".pdf", ".jpg", ".jpeg", ".json", ".webp"
        };

        public async Task<FileResponseDto> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File tải lên không hợp lệ hoặc rỗng.");

            if (file.Length > MaxFileSize)
                throw new ArgumentException($"Dung lượng file vượt quá giới hạn cho phép (tối đa 5MB). File hiện tại: {Math.Round((double)file.Length / (1024 * 1024), 2)}MB.");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new ArgumentException("Định dạng file không được hỗ trợ. Chỉ được phép tải lên các file: Word (.doc, .docx), tài liệu cố định (.pdf), .jpg, .jpeg, .json, .webp.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var storedFile = new StoredFile
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                Content = memoryStream.ToArray(),
                Length = file.Length
            };

            var createdFile = await _fileRepository.AddAsync(storedFile);

            return new FileResponseDto
            {
                Id = createdFile.Id,
                FileName = createdFile.FileName,
                Size = createdFile.Length,
                UploadedAt = createdFile.UploadedAt
            };
        }

        public async Task<FileDownloadDto?> GetFileForDownloadAsync(int id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null) return null;

            return new FileDownloadDto
            {
                Content = file.Content,
                ContentType = file.ContentType,
                FileName = file.FileName
            };
        }
    }
}