namespace DataAccess.Enums;

public class TokenDTO
{
   public string AccessToken { get; set; }
   public string RefreshToken { get; set; }
   public DateTime RefreshTokenExpiresAt { get; set; }
   
}