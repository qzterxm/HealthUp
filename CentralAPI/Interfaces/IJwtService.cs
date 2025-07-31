using System.Security.Claims;
using DataAccess.Enums;

namespace WebApplication1.Interfaces;

public interface IJwtService
{
    TokenDTO GenerateTokens(IEnumerable<Claim> claims,TimeSpan refreshTokenLifetime);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}