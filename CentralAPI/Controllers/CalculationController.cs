using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;
using DataAccess.Enums;
using System;
using System.Threading.Tasks;
using DataAccess.Interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly ICalculationService _calculationService;
        private readonly IUserRepository _userRepository;

        public CalculationController(ICalculationService calculationService, IUserRepository userRepository)
        {
            _calculationService = calculationService;
            _userRepository = userRepository;
        }

        [HttpPost("AddMeasurement")]
        public async Task<IActionResult> AddMeasurement([FromBody] HealthMeasurementDTO measurementDto)
        {
           
            if (measurementDto.UserId == Guid.Empty)
                return BadRequest("UserId is required.");

           
            if (measurementDto.Systolic.HasValue && (measurementDto.Systolic < _calculationService.MinSystolic || measurementDto.Systolic > _calculationService.MaxSystolic))
                return BadRequest("Systolic pressure outside the acceptable limits");

            if (measurementDto.Diastolic.HasValue && (measurementDto.Diastolic < _calculationService.MinDiastolic || measurementDto.Diastolic > _calculationService.MaxDiastolic))
                return BadRequest("Diastolic pressure outside the acceptable limits");

            if (measurementDto.HeartRate.HasValue && (measurementDto.HeartRate < _calculationService.MinHeartRate || measurementDto.HeartRate > _calculationService.MaxHeartRate))
                return BadRequest("Heart rate outside acceptable limits");
            
            measurementDto.Id = Guid.NewGuid();
            await _userRepository.AddMeasurement(measurementDto);
          

            return Ok(measurementDto);
        }
        
        [HttpPost("AddAnthropometry")]
        public async Task<IActionResult> AddAnthropometry([FromBody] AnthropometryDTO dto)
        {
            if (dto.Weight <= 0 || dto.Height <= 0)
                return BadRequest("Weight or Height must be greater than 0");

            var result = await _userRepository.AddAnthrometry(dto);
            if (result == 1)
                return Ok("Added anthropometry data ");
            else
                return StatusCode(500, "Failed to add anthropometry");
           
        }

        
        [HttpGet("GetAverage")]
        public async Task<IActionResult> GetAverages(Guid userId)
        {
            // Середній тиск та пульс 
            var avgBP = await _calculationService.AverageBP(userId);
            var avgHR = await _calculationService.AverageHR(userId);

           
            var anthropometries = await _userRepository.GetAnthropometries(userId);
            double imt = 0;

            if (anthropometries.Any())
            {
                var latest = anthropometries.OrderByDescending(a => a.MeasuredAt).First();
                if (latest.Height.HasValue && latest.Weight.HasValue)
                {
                    imt = await _calculationService.CalculateIMT(latest.Height.Value, latest.Weight.Value);
                }
            }

            return Ok(new
            {
                AverageBP = avgBP,
                AverageHR = avgHR,
                IMT = imt
            });
        }

    }
}
