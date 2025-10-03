using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;
using Mapster;
using WebApplication1.Enums;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpPost("create-user")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        var result = await _userService.CreateUser(user);
        return result 
            ? Ok(new { message = "User has been created successfully", success = true, data = new { id = user.Id, email = user.Email, role = user.UserRole } })
            : BadRequest(new { message = "User hasn't been created or already exists", success = false, data = (object)null });
    }

    [HttpGet("get-by-id")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetById(id);
        return user == null 
            ? NotFound(new { message = $"User with ID {id} not found", success = false, data = (object)null })
            : Ok(new { message = "User found", success = true, data = user.Adapt<User>() });
    }

    [HttpGet("get-user-by-email")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmail(email);
        return user == null
            ? NotFound(new { message = "User not found", success = false, data = (object)null })
            : Ok(new { message = "User found", success = true, data = user.Adapt<User>() });
    }

    [HttpGet("change-user-role")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangeUserRole(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { message = "Invalid user ID", success = false, data = (object)null });

        var result = await _userService.ChangeUserRole(id, UserRoles.Admin);
        return result
            ? Ok(new { message = $"User with ID {id} promoted to Admin", success = true, data = new { id, role = "Admin" } })
            : NotFound(new { message = $"User with ID {id} not found or role change failed", success = false, data = (object)null });
    }

    [HttpPut("update-user")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateUser([FromBody] User userDto)
    {
        var user = await _userService.GetById(userDto.Id);
        if (user == null)
            return NotFound(new { message = $"User with ID {userDto.Id} not found", success = false, data = (object)null });

        var result = await _userService.UpdateUser(user.Id, userDto.Adapt<User>());
        return result
            ? Ok(new { message = "User updated successfully", success = true, data = userDto })
            : StatusCode(500, new { message = "Update failed", success = false, data = (object)null });
    }

    [HttpDelete("delete-user")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUser(id);
        return result
            ? Ok(new { message = "User deleted successfully", success = true, data = (object)null })
            : NotFound(new { message = $"User with ID {id} not found", success = false, data = (object)null });
    }

    [HttpGet("get-all-users")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(new { message = "Users list", success = true, data = users });
    }
}
