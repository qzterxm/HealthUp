using System.Data;
using Dapper;
using DataAccess.DataAccess;
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
                var parameters = new DynamicParameters();
                parameters.Add("@Id", newFile.Id);
                parameters.Add("@UserId", newFile.UserId);
                parameters.Add("@FileName", newFile.FileName);
                parameters.Add("@ContentType", newFile.ContentType);
                parameters.Add("@FileData", newFile.FileData, DbType.Binary);
                parameters.Add("@UploadedAt", newFile.UploadedAt);

                var rowsAffected = await _dbAccessService.AddUserFile(parameters);

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

                var files = await _dbAccessService.GetUserFile(parameters);

                return files.FirstOrDefault();
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

                var rowsAffected = await _dbAccessService.DeleteUserFile(parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {FileName} for user {UserId}", fileName, userId);
                return false;
            }
        }
    }
}
