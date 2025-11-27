using DataAccess.Enums;
using DataAccess.Models;

namespace WebApplication1.Interfaces;

public interface IFileService
{
    Task<UserFile?> UploadFile(Guid userId, IFormFile file);
    Task<UserFile?> DownloadFile(Guid userId, string fileName);
    Task<bool> DeleteFile(Guid userId, string fileName);
    Task<object?> HandleFileOperationAsync(Guid userId, FileOperationType operation, Guid? fileId = null, IFormFile? file = null, Guid? visitId = null, Guid? noteId = null);
    Task<UserFile> AttachFileToVisitAsync(Guid visitId, IFormFile file, Guid? fileId = null);
    
}