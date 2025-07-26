using DataAccess.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Enums;
using WebApplication1.Interfaces;

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
    public async Task<bool> CreateUser(User user)
    {
        bool result = await _userService.CreateUser(user);
        return result;
    }
    
    [HttpGet("GetById")]
    [AllowAnonymous]
    public async Task<User?> GetById(Guid id)
    {
        var user = await _userService.GetById(id);
        return user;
        
    }

    [HttpGet("GetUserByEmail")]
    [AllowAnonymous]
    public async Task<User?> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmail(email);
        return user;
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

    
}