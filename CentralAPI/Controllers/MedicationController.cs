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

        public MedicationsController(IDbAccessService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public async Task<IActionResult> AddMedication([FromBody] Medication medication)
        {
            if (medication == null)
                return BadRequest(new { success = false, message = "Medication is required" });

            medication.Id = Guid.NewGuid();
            medication.CreatedAt = DateTime.UtcNow;
            medication.UpdatedAt = DateTime.UtcNow;

            var result = await _dbService.AddMedication(medication);

            if (result > 0)
                return Ok(new { success = true, message = "Medication added successfully", data = medication });

            return BadRequest(new { success = false, message = "Failed to add medication" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] Medication medication)
        {
            if (medication == null || id != medication.Id)
            {
                return BadRequest(new { success = false, message = "Invalid medication data" });
            }

            medication.UpdatedAt = DateTime.UtcNow;


            var result = await _dbService.UpdateMedication(medication);

            if (result > 0)
                return Ok(new { success = true, message = "Medication updated successfully", data = medication });

            return NotFound(new { success = false, message = "Medication not found or failed to update" });
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMedications(Guid userId)
        {
            var medications = await _dbService.GetMedications(userId);
            return Ok(new { success = true, data = medications });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            var result = await _dbService.DeleteMedication(id);
            if (result) return Ok(new { success = true, message = "Medication deleted successfully" });
            return NotFound(new { success = false, message = "Medication not found" });
        }
    }
}