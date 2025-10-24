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

    public fileController(IFileService fileService, ILogger<fileController> logger,  IDbAccessService dbAccessService)
    {
        _fileService = fileService;
        _logger = logger;
        _dbAccessService = dbAccessService;
    }

    [HttpPost ("upload-file")]
    public async Task<IActionResult> Handle([FromQuery] FileOperationType type, [FromQuery] Guid userId, [FromQuery] Guid? fileId, IFormFile? file)
    {
        try
        {
            var result = await _fileService.HandleFileOperationAsync(userId, type, fileId, file);

            return type switch
            {
                FileOperationType.Upload => Ok(new { success = true, message = "File uploaded successfully", data = result }),
                FileOperationType.Delete => Ok(new { success = true, message = "File deleted successfully" }),
                FileOperationType.Download => result is UserFile f
                    ? File(f.FileData, f.ContentType, f.FileName)
                    : NotFound(new { success = false, message = "File not found" }),
                _ => BadRequest(new { success = false, message = "Invalid operation" })
            };


        }
        catch (FileNotFoundException)
        {
            return NotFound(new { success = false, message = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File operation failed");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    
   

}