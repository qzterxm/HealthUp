using Microsoft.AspNetCore.Mvc;
using DataAccess.Interfaces; 
using DataAccess.Models;     
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class sleepController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<sleepController> _logger;

        public sleepController(IUserRepository userRepository, ILogger<sleepController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddSleep([FromBody] SleepDTO sleepDto)
        {
            if (sleepDto == null)
            {
                return BadRequest("Sleep data is null.");
            }

            if (sleepDto.UserId == Guid.Empty)
            {
                return BadRequest("UserId is required.");
            }

            try
            {
               
                sleepDto.CalculateDuration();
                sleepDto.CalculateSleepScore();

                var result = await _userRepository.AddSleepData(sleepDto);

                if (result > 0)
                {
                    return Ok(new { 
                        Message = "Sleep data added successfully", 
                        Id = sleepDto.Id // Повертаємо згенерований ID
                    });
                }
                else
                {
                    return StatusCode(500, "Failed to add sleep data to database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddSleep controller method.");
                return StatusCode(500, "Internal server error.");
            }
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSleepHistory(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("UserId is empty.");
            }

            try
            {
                var history = await _userRepository.GetSleepData(userId);
                
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sleep history for user {UserId}", userId);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}