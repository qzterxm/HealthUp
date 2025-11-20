
using Dapper;
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
    private readonly ILogger _logger;

    public calculationController(ICalculationService calculationService, IUserRepository userRepository,  ILogger<calculationController> logger)
    {
        _calculationService = calculationService;
        _userRepository = userRepository;
        _logger = logger;
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
    try
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
                
            return BadRequest(new { 
                message = "Validation failed", 
                success = false, 
                errors = errors,
                data = (object)null 
            });
        }

        if (dto.UserId == Guid.Empty)
            return BadRequest(new { message = "Valid UserId is required", success = false, data = (object)null });

        if (dto.Weight <= 0 || dto.Height <= 0)
            return BadRequest(new { message = "Weight and Height must be greater than 0", success = false, data = (object)null });

        var user = await _userRepository.GetById(dto.UserId);
        if (user == null)
        {
            return NotFound(new { 
                message = $"User with ID {dto.UserId} not found", 
                success = false, 
                data = (object)null 
            });
        }

        if (dto.MeasuredAt == default)
        {
            dto.MeasuredAt = DateTime.UtcNow;
        }

        if (dto.Age.HasValue && dto.Age.Value > 0)
        {
            user.Age = dto.Age.Value;
            await _userRepository.UpdateUser(dto.UserId, user);
        }

        _logger.LogInformation("Adding anthropometry for user {UserId}: Weight={Weight}, Height={Height}, Sugar={Sugar}, BloodType={BloodType}, Age={Age}", 
            dto.UserId, dto.Weight, dto.Height, dto.Sugar, dto.BloodType, dto.Age);

        var result = await _userRepository.AddAnthrometry(dto);
        
        return result > 0
            ? Ok(new { 
                message = "Anthropometry added successfully", 
                success = true, 
                data = new { 
                    userId = dto.UserId,
                    ageUpdated = dto.Age.HasValue
                } 
            })
            : StatusCode(500, new { message = "Failed to add anthropometry", success = false, data = (object)null });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding anthropometry for user {UserId}", dto?.UserId);
        return StatusCode(500, new { 
            message = "An error occurred while adding anthropometry", 
            success = false, 
            data = (object)null 
        });
    }
}
    
    
    [HttpGet("get-average")]
    public async Task<IActionResult> GetAverages([FromQuery] Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
                return BadRequest(new { message = "UserId is required", success = false, data = (object)null });

            _logger.LogInformation("=== GetAverages START for user: {UserId} ===", userId);

            var measurementsTask = _userRepository.GetMeasurements(userId);
            var anthropometriesTask = _userRepository.GetAnthropometries(userId);
            var userTask = _userRepository.GetById(userId);

            await Task.WhenAll(measurementsTask, anthropometriesTask, userTask);

            var allMeasurements = measurementsTask.Result;
            var allAnthropometries = anthropometriesTask.Result;
            var user = userTask.Result;
        
            _logger.LogInformation("Measurements count: {MeasurementsCount}", allMeasurements.Count);
            _logger.LogInformation("Anthropometries count: {AnthropometryCount}", allAnthropometries.Count);

            var latestAnthropometry = allAnthropometries.OrderByDescending(a => a.MeasuredAt).FirstOrDefault();
            double avgHR = allMeasurements.Any() ? allMeasurements.Average(m => m.HeartRate ?? 0) : 0;
            double avgSystolic = allMeasurements.Any() ? allMeasurements.Average(m => m.Systolic ?? 0) : 0;
            double avgDiastolic = allMeasurements.Any() ? allMeasurements.Average(m => m.Diastolic ?? 0) : 0;
            
            double latestHeight = latestAnthropometry?.Height ?? 0.0;
            double latestWeight = latestAnthropometry?.Weight ?? 0.0;
            double latestSugar = latestAnthropometry?.Sugar ?? 0.0;
            int? age = latestAnthropometry?.Age ?? user?.Age;
            
            _logger.LogInformation("Calculated values - LatestHeight: {LatestHeight}, LatestWeight: {LatestWeight}, LatestSugar: {LatestSugar}", 
                latestHeight, latestWeight, latestSugar);

            double imt = 0;
            if (latestHeight > 0 && latestWeight > 0)
            {
                double heightInMeters = latestHeight / 100.0; 
                imt = latestWeight / (heightInMeters * heightInMeters);
                _logger.LogInformation("IMT calculation: {Weight} / ({Height}/100)^2 = {IMT}", 
                    latestWeight, latestHeight, imt);
            }

            string bloodGroupString = GetBloodGroupString(latestAnthropometry?.BloodType);

            _logger.LogInformation("=== GetAverages END ===");

            return Ok(new
            {
                message = "Average and latest data retrieved",
                success = true,
                data = new
                {
                    averageHeartRate = Math.Round(avgHR, 1),
                    averageSystolic = (int)Math.Round(avgSystolic), 
                    averageDiastolic = (int)Math.Round(avgDiastolic), 
                    latestHeartRate = allMeasurements.OrderByDescending(m => m.MeasuredAt).FirstOrDefault()?.HeartRate ?? 0,
                    latestHeight = latestHeight,
                    latestWeight = latestWeight, 
                    bloodGroup = bloodGroupString,
                    imt = Math.Round(imt, 1),
                    latestSugar = latestSugar
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAverages for user {UserId}", userId);
            return StatusCode(500, new { 
                message = "An error occurred while retrieving data", 
                success = false, 
                data = (object)null 
            });
        }
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

[HttpPost("update-health-data")]
public async Task<IActionResult> UpdateHealthData([FromBody] UpdateUserHealthDataDTO healthData)
{
    try
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
                
            return BadRequest(new { 
                message = "Validation failed", 
                success = false, 
                errors = errors,
                data = (object)null 
            });
        }

        if (healthData.UserId == Guid.Empty)
            return BadRequest(new { message = "Valid UserId is required", success = false, data = (object)null });

        var user = await _userRepository.GetById(healthData.UserId);
        if (user == null)
            return NotFound(new { message = "User not found", success = false, data = (object)null });

        _logger.LogInformation("Updating health data for user: {UserId}", healthData.UserId);

        var result = await _userRepository.UpdateUserHealthData(healthData);
        
        if (result)
        {
            return Ok(new { 
                message = "Health data updated successfully", 
                success = true, 
                data = new { 
                    userId = healthData.UserId,
                    updatedFields = new {
                        Age = healthData.Age,
                        Gender = healthData.Gender.ToString(),
                        DateOfBirth = healthData.DateOfBirth?.ToString("yyyy-MM-dd"),
                        Country = healthData.Country,
                        PhoneNumber = healthData.PhoneNumber,
                        BloodType = healthData.BloodType?.ToString(),
                        SugarLevel = healthData.SugarLevel,
                        Height = healthData.Height,
                        Weight = healthData.Weight
                    }
                } 
            });
        }
        else
        {
            return StatusCode(500, new { 
                message = "Failed to update health data", 
                success = false, 
                data = (object)null 
            });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating health data for user {UserId}", healthData.UserId);
        return StatusCode(500, new { 
            message = "An error occurred while updating health data", 
            success = false, 
            data = (object)null 
        });
    }
}

}