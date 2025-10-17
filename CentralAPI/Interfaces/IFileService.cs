using DataAccess.Models;

namespace WebApplication1.Interfaces;

public interface IFileService
{
    Task<UserFile?> UploadFile(Guid userId, IFormFile file);
    Task<UserFile?> DownloadFile(Guid userId, string fileName);
    Task<bool> DeleteFile(Guid userId, string fileName);
}