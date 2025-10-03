using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordHelperService _passwordHelper;
    private readonly IJwtService _jwtHelper;

    public AuthController(IAuthService authService, IPasswordHelperService passwordHelper, IJwtService jwtHelper)
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
            return BadRequest(new { message = "Email is required", success = false, data = (object)null });
        if (string.IsNullOrEmpty(registrationUser.Password))
            return BadRequest(new { message = "Password is required", success = false, data = (object)null });
        if (string.IsNullOrEmpty(registrationUser.UserName))
            return BadRequest(new { message = "Username is required", success = false, data = (object)null });

        if (await _authService.GetUserByEmail(registrationUser.Email) != null)
            return Conflict(new { message = "Email already in use", success = false, data = (object)null });

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
            return BadRequest(new { message = "Registration failed", success = false, data = (object)null });

        return Ok(new { message = "Registration successful", success = true, data = new { id = user.Id, email = user.Email, role = user.UserRole } });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
    {
        if (string.IsNullOrEmpty(loginUser.Email))
            return BadRequest(new { message = "Email is required", success = false, data = (object)null });
        if (string.IsNullOrEmpty(loginUser.Password))
            return BadRequest(new { message = "Password is required", success = false, data = (object)null });

        var user = await _authService.GetUserByEmail(loginUser.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials", success = false, data = (object)null });

        if (!_passwordHelper.VerifyPassword(loginUser.Password, user.Password))
            return Unauthorized(new { message = "Invalid password", success = false, data = (object)null });

        var tokens = await _authService.Login(user, loginUser.RememberMe);

        return Ok(new
        {
            message = "Login successful",
            success = true,
            data = new { accessToken = tokens.AccessToken, refreshToken = tokens.RefreshToken, expiresAt = tokens.RefreshTokenExpiresAt }
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshAccessToken([FromBody] TokenDTO tokensModel)
    {
        if (string.IsNullOrEmpty(tokensModel.AccessToken))
            return BadRequest(new { message = "Access token is required", success = false, data = (object)null });
        if (string.IsNullOrEmpty(tokensModel.RefreshToken))
            return BadRequest(new { message = "Refresh token is required", success = false, data = (object)null });

        try
        {
            var principal = _jwtHelper.GetPrincipalFromExpiredToken(tokensModel.AccessToken);
            if (principal == null)
                return BadRequest(new { message = "Invalid token", success = false, data = (object)null });

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userGuid))
                return BadRequest(new { message = "Invalid token", success = false, data = (object)null });

            var user = await _authService.GetUserById(userGuid);
            if (user == null)
                return Unauthorized(new { message = "User not found", success = false, data = (object)null });

            var newTokens = await _authService.RefreshAccessToken(user);
            return Ok(new
            {
                message = "Token refreshed",
                success = true,
                data = new { accessToken = newTokens.AccessToken, refreshToken = newTokens.RefreshToken, expiresAt = newTokens.RefreshTokenExpiresAt }
            });
        }
        catch (SecurityTokenMalformedException)
        {
            return BadRequest(new { message = "Invalid token format", success = false, data = (object)null });
        }
        catch (SecurityTokenExpiredException)
        {
            return BadRequest(new { message = "Token expired", success = false, data = (object)null });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while refreshing token", success = false, data = (object)null });
        }
    }
}
