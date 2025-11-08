
using DataAccess.Enums;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;


namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class calculationController : ControllerBase
{
    private readonly ICalculationService _calculationService;
    private readonly IUserRepository _userRepository;

    public calculationController(ICalculationService calculationService, IUserRepository userRepository)
    {
        _calculationService = calculationService;
        _userRepository = userRepository;
    }

    [HttpPost("add-measurement")]
    public async Task<IActionResult> AddMeasurement([FromBody] HealthMeasurementDTO measurementDto)
    {
        if (measurementDto.UserId == Guid.Empty)
            return BadRequest(new { message = "UserId is required", success = false, data = (object)null });

        measurementDto.Id = Guid.NewGuid();
        await _userRepository.AddMeasurement(measurementDto);

        return Ok(new { message = "Measurement added", success = true, data = measurementDto });
    }

    [HttpPost("add-anthropometry")]
    public async Task<IActionResult> AddAnthropometry([FromBody] AnthropometryDTO dto)
    {
        if (dto.Weight <= 0 || dto.Height <= 0)
            return BadRequest(new { message = "Weight or Height must be greater than 0", success = false, data = (object)null });

        if (dto.MeasuredAt == default)
        {
            dto.MeasuredAt = DateTime.UtcNow;
        }

        var result = await _userRepository.AddAnthrometry(dto);
        return result > 0
            ? Ok(new { message = "Anthropometry added", success = true, data = new { id = dto.UserId } })
            : StatusCode(500, new { message = "Failed to add anthropometry", success = false, data = (object)null });
    }
    
    
[HttpGet("get-average")]
public async Task<IActionResult> GetAverages([FromQuery] Guid userId)
{
    if (userId == Guid.Empty)
        return BadRequest(new { message = "UserId is required", success = false, data = (object)null });

    var measurementsTask = _userRepository.GetMeasurements(userId);
    var anthropometriesTask = _userRepository.GetAnthropometries(userId); // Отримуємо всі записи
  
    await Task.WhenAll(measurementsTask, anthropometriesTask);

    var allMeasurements = measurementsTask.Result;
    var allAnthropometries = anthropometriesTask.Result;
    
    var latestAnthropometry = allAnthropometries.OrderByDescending(a => a.MeasuredAt).FirstOrDefault();
    
    double avgHR = allMeasurements.Any() ? allMeasurements.Average(m => m.HeartRate ?? 0) : 0;
    double avgSystolic = allMeasurements.Any() ? allMeasurements.Average(m => m.Systolic ?? 0) : 0;
    double avgDiastolic = allMeasurements.Any() ? allMeasurements.Average(m => m.Diastolic ?? 0) : 0;
    
    double latestHeight = latestAnthropometry?.Height ?? 0.0;
    double latestWeight = latestAnthropometry?.Weight ?? 0.0;
    double latestSugar = latestAnthropometry?.Sugar ?? 0.0;
    
    double imt = 0;
    if (latestHeight > 0 && latestWeight > 0)
    {
        imt = await _calculationService.CalculateIMT(latestHeight, latestWeight);
    }

    string bloodGroupString = GetBloodGroupString(latestAnthropometry?.BloodType);

    return Ok(new
    {
        message = "Average and latest data retrieved",
        success = true,
        data = new
        {
            averageHeartRate = avgHR,
            averageSystolic = (int)Math.Round(avgSystolic), 
            averageDiastolic = (int)Math.Round(avgDiastolic), 
            latestHeartRate = allMeasurements.OrderByDescending(m => m.MeasuredAt).FirstOrDefault()?.HeartRate ?? 0,
            latestHeight = (int)Math.Round(latestHeight),
            latestWeight = (int)Math.Round(latestWeight), 
            bloodGroup = bloodGroupString,
            imt = imt,
            latestSugar = latestSugar
        }
    });
}

private string GetBloodGroupString(BloodType? type)
{
    return type switch
    {
        BloodType.A_Positive => "A+",
        BloodType.A_Negative => "A-",
        BloodType.B_Positive => "B+",
        BloodType.B_Negative => "B-",
        BloodType.AB_Positive => "AB+",
        BloodType.AB_Negative => "AB-",
        BloodType.O_Positive => "O+",
        BloodType.O_Negative => "O-",
        _ => "N/A"
    };
}
    
}