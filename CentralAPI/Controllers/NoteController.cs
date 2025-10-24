using DataAccess.DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;
[ApiController]
[Microsoft.AspNetCore.Components.Route("[controller]")]


public class NoteController:  ControllerBase
{
    private readonly ILogger<fileController> _logger;
    private readonly IDbAccessService _dbAccessService;

    public NoteController(ILogger<fileController> logger, IDbAccessService dbAccessService)
    {
        _logger = logger;
        _dbAccessService = dbAccessService;
        
    }
    
    [HttpPost("add-note")]
    public async Task<IActionResult> AddNote([FromBody] UserNoteDTO note)
    {
        await _dbAccessService.AddUserNote(note);
        return Ok(new { success = true });
    }

    [HttpGet("get-note")]
    public async Task<IActionResult> GetNotes(Guid userId)
    {
        var notes = await _dbAccessService.GetUserNotes(userId);
        return Ok(notes);
    }

    [HttpDelete("delete-note")]
    public async Task<IActionResult> DeleteNote(Guid userId, Guid noteId)
    {
        var success = await _dbAccessService.DeleteUserNote(userId, noteId);
        return Ok(new { success });
    }
}