using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Enums;
using WebApplication1.Interfaces;
using Mapster;
namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]

public class UserController: ControllerBase
{ 
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }
    [HttpPost("CreateUser")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        bool result = await _userService.CreateUser(user);
        return result ? Ok(true) : BadRequest(false);
    }
    
    [HttpGet("GetById")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetById(id);
        return user == null ? NotFound($"User with ID {id} not found or smth went wrong.") : Ok(user.Adapt<UserDTO>());
        
    }

    [HttpGet("GetUserByEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmail(email);
        return user == null ? NotFound("User not found.") : Ok(user.Adapt<UserDTO>());
    }

    [HttpGet("ChangeUserRole")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangeUserRole(Guid id)
    {
        if (id == Guid.Empty || id == null)
        {
            return BadRequest("Invalid user ID.");
        }

        var result = await _userService.ChangeUserRole(id, UserRoles.Admin);
        return result
            ? Ok($"User with ID {id} has been promoted to Admin.")
            : NotFound($"User with ID {id} not found or role change failed or smth went wrong.");
    }
    [AllowAnonymous]
    [HttpPut("UpdateUser")]
    public async Task<IActionResult> UpdateUser(UserDTO userDto)
    {
        var user = await _userService.GetById(userDto.Id);
        if(user == null)
        {
            return NotFound($"User with ID {userDto.Id} not found.");
        }

        var result = await _userService.UpdateUser(user.Id, userDto.Adapt<User>());

        return Ok(result);
    }

    [HttpDelete("DeleteUser")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUser(id);
        return result ? Ok($"User deleted") : NotFound($"User with ID {id} not found or role change failed.");
    }

    [HttpGet("GetAllUsers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }
    
    
}