using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Enums;
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

        var result = await _userRepository.AddAnthrometry(dto);
        return result == 1
            ? Ok(new { message = "Anthropometry added", success = true, data = new { id = dto.UserId } })
            : StatusCode(500, new { message = "Failed to add anthropometry", success = false, data = (object)null });
    }

    [HttpGet("get-average")]
    public async Task<IActionResult> GetAverages(Guid userId)
    {
        var avgBP = await _calculationService.AverageBP(userId);
        var avgHR = await _calculationService.AverageHR(userId);

        var anthropometries = await _userRepository.GetAnthropometries(userId);
        double imt = 0;

        if (anthropometries.Any())
        {
            var latest = anthropometries.OrderByDescending(a => a.MeasuredAt).First();
            if (latest.Height.HasValue && latest.Weight.HasValue)
                imt = await _calculationService.CalculateIMT(latest.Height.Value, latest.Weight.Value);
        }

        return Ok(new { message = "Average calculated", success = true, data = new { averageBP = avgBP, averageHR = avgHR, imt } });
    }
}
