using System.Data;
using Dapper;
using DataAccess.DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebApplication1.Interfaces;

namespace WebApplication1.Implementation
{
    public class FileService : IFileService
    {
        private readonly IDbAccessService _dbAccessService;
        private readonly ILogger<FileService> _logger;

        public FileService(IDbAccessService dbAccessService, ILogger<FileService> logger)
        {
            _dbAccessService = dbAccessService;
            _logger = logger;
        }

        
        public async Task<UserFile?> UploadFile(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            byte[] fileData;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileData = ms.ToArray();
            }

            var newFile = new UserFile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileData = fileData,
                UploadedAt = DateTime.UtcNow
            };

            try
            {
                
                var rowsAffected = await _dbAccessService.AddUserFile(newFile);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("File {FileName} uploaded by user {UserId}", newFile.FileName, userId);
                    return newFile;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FileName} for user {UserId}", newFile.FileName, userId);
                throw;
            }
        }

        public async Task<UserFile?> DownloadFile(Guid userId, string fileName)
        {
            try
            {
                var parameters = new DynamicParameters(); 
                parameters.Add("@UserId", userId);
                parameters.Add("@FileName", fileName);

                return await Task.FromResult<UserFile?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file {FileName} for user {UserId}", fileName, userId);
                throw;
            }
        }

        public async Task<bool> DeleteFile(Guid userId, string fileName)
        {
           
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@FileName", fileName);

                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {FileName} for user {UserId}", fileName, userId);
                return false;
            }
        }
        
       
       public async Task<object?> HandleFileOperationAsync(Guid userId, FileOperationType operation, Guid? fileId = null, IFormFile? file = null, Guid? visitId = null) // <--- ДОДАНО visitId
        {
            switch (operation)
            {
                case FileOperationType.Upload:
                    if (file == null || file.Length == 0)
                        throw new ArgumentException("File is empty or missing.");

                    byte[] fileData;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileData = ms.ToArray();
                    }

                    var newFile = new UserFile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileData = fileData,
                        UploadedAt = DateTime.UtcNow,
                        VisitId = visitId // <--- ВАЖЛИВО: Присвоюємо отриманий VisitId
                    };

                    await _dbAccessService.AddUserFile(newFile);
                    
                    _logger.LogInformation("File uploaded via HandleFileOperationAsync. Name: {Name}, VisitId: {VisitId}", newFile.FileName, newFile.VisitId);
                    
                    return newFile;

                case FileOperationType.Download:
                    if (fileId == null)
                        throw new ArgumentNullException(nameof(fileId));

                    var fileRecord = await _dbAccessService.GetUserFileById(fileId.Value, userId);
                    if (fileRecord == null)
                        throw new FileNotFoundException("File not found or access denied.");

                    return fileRecord;

                case FileOperationType.Delete:
                    if (fileId == null)
                        throw new ArgumentNullException(nameof(fileId));
                    
                    var rows = await _dbAccessService.DeleteUserFileById(fileId.Value, userId);
                    return rows > 0;

                default:
                    throw new NotSupportedException("Unsupported file operation.");
            }
        }
        
        public async Task<UserFile> AttachFileToVisitAsync(Guid visitId, IFormFile file, Guid? fileId = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or missing.");

            byte[] fileData;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileData = ms.ToArray();
            }

            var newFile = new UserFile
            {
                Id = fileId ?? Guid.NewGuid(),
                // УВАГА: Тут бажано б отримати UserId, але якщо логіка дозволяє null або береться з візиту пізніше - ок. 
                // Але краще, щоб у UserFile завжди був UserId.
                VisitId = visitId, 
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileData = fileData,
                UploadedAt = DateTime.UtcNow
            };

            await _dbAccessService.AddUserFile(newFile);

            return newFile;
        }  
    }
}