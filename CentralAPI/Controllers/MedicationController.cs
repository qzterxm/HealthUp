using System;
using System.Text.Json;
using System.Threading.Tasks;
using DataAccess.DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicationsController : ControllerBase
    {
        private readonly IDbAccessService _dbService;
        private readonly ILogger<MedicationsController> _logger;

        public MedicationsController(IDbAccessService dbService, ILogger<MedicationsController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddMedication([FromBody] Medication medication)
        {
            try
            {
                _logger.LogInformation("Adding new medication: {MedicationName}", medication?.NameOfMedication);

                if (medication == null)
                    return BadRequest(new { success = false, message = "Medication is required" });

                // Валідація обов'язкових полів
                if (string.IsNullOrEmpty(medication.UserId))
                    return BadRequest(new { success = false, message = "UserId is required" });

                if (string.IsNullOrEmpty(medication.NameOfMedication))
                    return BadRequest(new { success = false, message = "Medication name is required" });

                // Встановлюємо значення за замовчуванням
                if (medication.StartDate == default)
                    medication.StartDate = DateTime.Today;

                if (string.IsNullOrEmpty(medication.Duration))
                    medication.Duration = "1 month";

                var result = await _dbService.AddMedication(medication);

                if (result > 0)
                {
                    _logger.LogInformation("Medication added successfully with ID: {MedicationId}", medication.Id);
                    return Ok(new { success = true, message = "Medication added successfully", data = medication });
                }

                _logger.LogWarning("Failed to add medication - no rows affected");
                return BadRequest(new { success = false, message = "Failed to add medication" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medication");
                return StatusCode(500, new { success = false, message = $"Failed to add medication: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] Medication medication)
        {
            try
            {
                _logger.LogInformation("Updating medication with ID: {MedicationId}", id);

                if (medication == null || id != medication.Id)
                {
                    return BadRequest(new { success = false, message = "Invalid medication data" });
                }

                medication.UpdatedAt = DateTime.UtcNow;

                var result = await _dbService.UpdateMedication(medication);

                if (result > 0)
                {
                    _logger.LogInformation("Medication updated successfully");
                    return Ok(new { success = true, message = "Medication updated successfully", data = medication });
                }

                _logger.LogWarning("Medication not found or failed to update: {MedicationId}", id);
                return NotFound(new { success = false, message = "Medication not found or failed to update" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medication {MedicationId}", id);
                return StatusCode(500, new { success = false, message = $"Failed to update medication: {ex.Message}" });
            }
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMedications(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting medications for user: {UserId}", userId);
                
                var medications = await _dbService.GetMedications(userId);
                
                _logger.LogInformation("Returning {Count} medications for user {UserId}", medications.Count, userId);
                
                return Ok(new { success = true, data = medications });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medications for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = $"Failed to get medications: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting medication with ID: {MedicationId}", id);
                
                var result = await _dbService.DeleteMedication(id);
                
                if (result) 
                {
                    _logger.LogInformation("Medication deleted successfully");
                    return Ok(new { success = true, message = "Medication deleted successfully" });
                }
                
                _logger.LogWarning("Medication not found: {MedicationId}", id);
                return NotFound(new { success = false, message = "Medication not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication {MedicationId}", id);
                return StatusCode(500, new { success = false, message = $"Failed to delete medication: {ex.Message}" });
            }
        }
    }
}