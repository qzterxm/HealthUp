using System.Data;
using Dapper;
using DataAccess.DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

[ApiController]
[Route("[controller]")]
public class doctorvisitController : ControllerBase
{
    private readonly IDbAccessService _dbAccessService;
    private readonly ILogger<doctorvisitController> _logger;
    private readonly IFileService _fileService;

    public doctorvisitController(IDbAccessService dbAccessService, ILogger<doctorvisitController> logger,  IFileService fileService)
    {
        _dbAccessService = dbAccessService;
        _logger = logger;
        _fileService = fileService;
    }
    [HttpPost ("add-visit")]
    public async Task<IActionResult> AddVisit([FromBody] DoctorVisit visit)
    {
        if (visit == null)
            return BadRequest(new { success = false, message = "Invalid visit data." });

        try
        {
            if (visit.VisitedAt == default)
                visit.VisitedAt = DateTime.UtcNow;

            var visitId = await _dbAccessService.AddDoctorVisit(visit);

            return Ok(new
            {
                success = true,
                message = "Visit recorded successfully",
                visitId = visit.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add doctor visit for user {UserId}", visit.UserId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


    [HttpGet("get-visits")]
    public async Task<IActionResult> GetVisits(Guid userId)
    {
        var visits = await _dbAccessService.GetDoctorVisits(userId);
        return Ok(visits);
    }
    
    [HttpPost("attach-file")]
    public async Task<IActionResult> AttachFile([FromQuery] Guid visitId, [FromQuery] Guid? fileId, IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "File is missing or empty." });

        try
        {
            var uploadedFile = await _fileService.AttachFileToVisitAsync(visitId, file, fileId);

            return Ok(new
            {
                success = true,
                message = "File attached to visit successfully",
                data = uploadedFile
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach file to visit {VisitId}", visitId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
