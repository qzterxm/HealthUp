using DataAccess.DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

[ApiController]
[Route("[controller]")]
public class fileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<fileController> _logger;
    private readonly IDbAccessService _dbAccessService;

    public fileController(IFileService fileService, ILogger<fileController> logger, IDbAccessService dbAccessService)
    {
        _fileService = fileService;
        _logger = logger;
        _dbAccessService = dbAccessService;
    }

    [HttpGet("user-files")]
    public async Task<IActionResult> GetUserFiles([FromQuery] Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting files for user: {UserId}", userId);
            
            var files = await _dbAccessService.GetUserFilesByUserId(userId);
            
            _logger.LogInformation("Retrieved {Count} files for user {UserId}", files.Count, userId);
            
            return Ok(new { 
                success = true, 
                message = "Files retrieved successfully",
                data = files 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user files for user {UserId}", userId);
            return StatusCode(500, new { 
                success = false, 
                message = "Error retrieving files" 
            });
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromQuery] Guid userId, IFormFile file, [FromQuery] Guid? visitId = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "File is empty or missing" });
            }

            _logger.LogInformation("Uploading file for user: {UserId}, File: {FileName}", userId, file.FileName);

            var result = await _fileService.HandleFileOperationAsync(userId, FileOperationType.Upload, null, file);

            return Ok(new { 
                success = true, 
                message = "File uploaded successfully", 
                data = result 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for user {UserId}", userId);
            return StatusCode(500, new { 
                success = false, 
                message = ex.Message 
            });
        }
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile([FromQuery] Guid userId, [FromQuery] Guid fileId)
    {
        try
        {
            _logger.LogInformation("Downloading file {FileId} for user {UserId}", fileId, userId);

            var result = await _fileService.HandleFileOperationAsync(userId, FileOperationType.Download, fileId, null);

            if (result is UserFile file)
            {
                return File(file.FileData, file.ContentType, file.FileName);
            }

            return NotFound(new { success = false, message = "File not found" });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { success = false, message = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId} for user {UserId}", fileId, userId);
            return StatusCode(500, new { 
                success = false, 
                message = ex.Message 
            });
        }
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile([FromQuery] Guid userId, [FromQuery] Guid fileId)
    {
        try
        {
            _logger.LogInformation("Deleting file {FileId} for user {UserId}", fileId, userId);

            var result = await _fileService.HandleFileOperationAsync(userId, FileOperationType.Delete, fileId, null);

            return Ok(new { 
                success = true, 
                message = "File deleted successfully" 
            });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { success = false, message = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId} for user {UserId}", fileId, userId);
            return StatusCode(500, new { 
                success = false, 
                message = ex.Message 
            });
        }
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetFileInfo([FromQuery] Guid userId, [FromQuery] Guid fileId)
    {
        try
        {
            _logger.LogInformation("Getting file info for file {FileId} and user {UserId}", fileId, userId);

            var file = await _dbAccessService.GetUserFileById(fileId, userId);
            
            if (file == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            var fileInfo = new
            {
                file.Id,
                file.FileName,
                file.ContentType,
                file.UploadedAt,
                file.UserId,
                file.VisitId
            };

            return Ok(new { 
                success = true, 
                message = "File info retrieved successfully",
                data = fileInfo 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for file {FileId} and user {UserId}", fileId, userId);
            return StatusCode(500, new { 
                success = false, 
                message = ex.Message 
            });
        }
    }
}