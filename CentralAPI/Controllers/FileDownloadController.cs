using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

[ApiController]
[Route("[controller]")]
public class fileController : ControllerBase
{
    private readonly ILogger<fileController> _logger;
    private readonly IFileService _fileService;

    public fileController(ILogger<fileController> logger, IFileService fileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is empty", success = false, data = (object)null });

        try
        {
            var fileRecord = await _fileService.UploadFile(Guid.Empty, file); 

            if (fileRecord == null)
            {
                _logger.LogWarning("File {FileName} could not be recorded in DB.", file.FileName);
           
                return Ok(new
                {
                    message = "File uploaded.",
                    success = true,
                    data = new { fileName = file.FileName }
                });
            }

            return Ok(new
            {
                message = "File uploaded successfully",
                success = true,
                data = new { id = fileRecord.Id, fileName = fileRecord.FileName }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed for file {FileName}.", file?.FileName ?? "null");
            return StatusCode(500, new 
            { 
                message = "Internal server error during upload.", 
                success = false, 
                error = ex.Message
            });
        }
    }

[HttpGet("download")]
public async Task<IActionResult> Download(string fileName)
{
    try
    {
        var fileRecord = await _fileService.DownloadFile(Guid.Empty, fileName);

        if (fileRecord == null)
            return NotFound(new { message = "File not found.", success = false, data = (object)null });

        return File(fileRecord.FileData, fileRecord.ContentType, fileRecord.FileName);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Download failed for file {FileName}.", fileName);
        return StatusCode(500, new 
        { 
            message = "Internal server error during download.", 
            success = false, 
            error = ex.Message
        });
    }
}

[HttpDelete("delete")]
public async Task<IActionResult> Delete(string fileName)
{
    if (string.IsNullOrWhiteSpace(fileName))
        return BadRequest(new { message = "File name is required.", success = false });

    try
    {
        var result = await _fileService.DeleteFile(Guid.Empty, fileName);

        if (!result)
        {
            _logger.LogWarning("File {FileName} was not found in DB for deletion.", fileName);
         return Ok(new
            {
                message = $"File {fileName} deleted.",
                success = true
            });
        }

        return Ok(new { message = $"File {fileName} deleted successfully.", success = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Delete failed for file {FileName}.", fileName);
        return StatusCode(500, new
        {
            message = "Internal server error during delete.",
            success = false,
            error = ex.Message
        });
    }
}
//TODO: РОЗІБРАТИСЬ ЧОМУ fileRecord == null та добавити тип крові
}
