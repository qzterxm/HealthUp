using System.Security.Claims;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    private readonly IPasswordHelperService _passwordHelper;
    private readonly IJwtService _jwtHelper;

    public AuthController(
        IAuthService authService,
         IPasswordHelperService passwordHelper,
        IJwtService jwtHelper)
    {
        _authService = authService;
          _passwordHelper = passwordHelper;
        _jwtHelper = jwtHelper;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationUser registrationUser)
    {
        if (string.IsNullOrEmpty(registrationUser.Email))
            return BadRequest("Email is required");
        if (string.IsNullOrEmpty(registrationUser.Password))
            return BadRequest("Password is required");
        if (string.IsNullOrEmpty(registrationUser.UserName))
            return BadRequest("Username is required");

        if (await _authService.GetUserByEmail(registrationUser.Email) != null)
            return Conflict("Email already in use");

        var hashedPassword = _passwordHelper.HashPassword(registrationUser.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = registrationUser.Email,
            UserName = registrationUser.UserName,
            Password = hashedPassword,
            UserRole = registrationUser.Role
        };

        var result = await _authService.Register(user);
        if (!result)
            return BadRequest("Registration failed");


        return Ok(new
        {
            Message = "Registration successful.",
            Success = true
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
    {
        if (string.IsNullOrEmpty(loginUser.Email))
            return BadRequest("Email is required");
        if (string.IsNullOrEmpty(loginUser.Password))
            return BadRequest("Password is required");

        var user = await _authService.GetUserByEmail(loginUser.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");


        if (!_passwordHelper.VerifyPassword(loginUser.Password, user.Password))
            return Unauthorized("Invalid password");

        var tokens = await _authService.Login(user, loginUser.RememberMe);

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshAccessToken([FromBody] TokenDTO tokensModel)
    {
        if (string.IsNullOrEmpty(tokensModel.AccessToken))
            return BadRequest("Access token is required");
        if (string.IsNullOrEmpty(tokensModel.RefreshToken))
            return BadRequest("Refresh token is required");

        try
        {
            var principal = _jwtHelper.GetPrincipalFromExpiredToken(tokensModel.AccessToken);
            if (principal == null)
                return BadRequest("Invalid token");

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userGuid))
                return BadRequest("Invalid token");

            var user = await _authService.GetUserById(userGuid);
            if (user == null)
                return Unauthorized("User not found");

            var newTokens = await _authService.RefreshAccessToken(user);
            return Ok(new TokenDTO
            {
                AccessToken = newTokens.AccessToken,
                RefreshToken = newTokens.RefreshToken,
                RefreshTokenExpiresAt = newTokens.RefreshTokenExpiresAt,
            });
        }
        catch (SecurityTokenMalformedException)
        {
            return BadRequest("Invalid token format");
        }
        catch (SecurityTokenExpiredException)
        {
            return BadRequest("Token expired");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while refreshing token");
        }
    }
}

    
