using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Interfaces;

namespace WebApplication1.Implementation;

public class JwtService : IJwtService
{
    private readonly byte[] _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public JwtService(IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:SecretKey"] ?? 
                       throw new ArgumentNullException(nameof(configuration), "JwtSettings:SecretKey is missing in configuration");
        _secretKey = Encoding.UTF8.GetBytes(secretKey);
        
        _issuer = configuration["JwtSettings:Issuer"] ?? 
                 throw new ArgumentNullException(nameof(configuration), "JwtSettings:Issuer is missing in configuration");
        _audience = configuration["JwtSettings:Audience"] ?? 
                   throw new ArgumentNullException(nameof(configuration), "JwtSettings:Audience is missing in configuration");
        
        if (!int.TryParse(configuration["JwtSettings:TokenExpiryMinutes"], out _accessTokenExpiryMinutes))
        {
            _accessTokenExpiryMinutes = 60;
        }
        
        if (!int.TryParse(configuration["JwtSettings:RefreshTokenExpiryDays"], out _refreshTokenExpiryDays))
        {
            _refreshTokenExpiryDays = 7;
        }
    }

    public TokenDTO GenerateTokens(IEnumerable<Claim> claims, TimeSpan? refreshTokenLifetime)
    {
        if (claims == null) throw new ArgumentNullException(nameof(claims));

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);
        var refreshTokenExpiresAt = refreshTokenLifetime.HasValue
            ? DateTime.UtcNow.Add(refreshTokenLifetime.Value)
            : DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        var accessToken = GenerateAccessToken(claims, accessTokenExpiresAt);
        var refreshToken = GenerateRefreshToken();

        return new TokenDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
        };
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_secretKey), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
    
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or whitespace", nameof(token));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = _audience,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_secretKey),
            ValidateLifetime = false 
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }
}