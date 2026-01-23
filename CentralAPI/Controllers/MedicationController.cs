using Microsoft.AspNetCore.Mvc;
using DataAccess.DataAccess;
using DataAccess.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationsController : ControllerBase
    {
        private readonly IDbAccessService _dbAccessService;

        public MedicationsController(IDbAccessService dbAccessService)
        {
            _dbAccessService = dbAccessService;
        }

        [HttpPost]
        public async Task<IActionResult> AddMedication([FromBody] Medication medication)
        {
            if (medication == null)
            {
                return BadRequest("Medication data is null");
            }

            // ВИПРАВЛЕННЯ: Перевіряємо Guid.Empty замість string.IsNullOrEmpty
            if (medication.UserId == Guid.Empty)
            {
                return BadRequest("UserId is required");
            }

            try
            {
                await _dbAccessService.AddMedication(medication);
                return Ok(new { success = true, message = "Medication added successfully", data = medication });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to add medication: {ex.Message}" });
            }
        }

        // ВИПРАВЛЕННЯ: Приймаємо Guid, а не string
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMedications(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid User ID");
            }

            try
            {
                // Передаємо Guid напряму в сервіс
                var medications = await _dbAccessService.GetMedications(userId);
                return Ok(new { success = true, data = medications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error getting medications: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] Medication medication)
        {
            if (id == Guid.Empty || medication.Id != id)
            {
                return BadRequest("Invalid ID");
            }

            try
            {
                await _dbAccessService.UpdateMedication(medication);
                return Ok(new { success = true, message = "Medication updated successfully", data = medication });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to update medication: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid ID");
            }

            try
            {
                var result = await _dbAccessService.DeleteMedication(id);
                if (result)
                {
                    return Ok(new { success = true, message = "Medication deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Medication not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting medication: {ex.Message}" });
            }
        }
    }
}