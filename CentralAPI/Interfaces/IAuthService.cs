using DataAccess.Enums;
using DataAccess.Models;

namespace WebApplication1.Interfaces;

public interface IAuthService 
{
    Task<TokenDTO> Login(User user, bool rememberMe);
    Task<bool> Register(User user);
    Task<TokenDTO> RefreshAccessToken(User user);
    Task<User?> GetUserByEmail(string email);
    Task<User> GetUserById(Guid id);
   
}