using System.Security.Claims;
using DataAccess.Enums;
using WebApplication1.Interfaces;

namespace WebApplication1.Implementation;

   public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtHelper;

        public AuthService(IUserService userService, IJwtService jwtHelper)
        {
            _userService = userService;
            _jwtHelper = jwtHelper;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }
            return await _userService.GetUserByEmail(email);
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _userService.GetById(id);
        }

        public async Task<bool> Register(User user)
        {
            return await _userService.CreateUser(user);
        }

        public async Task<TokenDTO> Login(User user, bool rememberMe)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null");

            if (user.Id == Guid.Empty)
                throw new ArgumentException("User ID is invalid", nameof(user.Id));

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("User email is required", nameof(user.Email));

            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("User name is required", nameof(user.UserName));
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.UserRole.ToString())
            };
            var refreshTokenLifetime = rememberMe
                ? TimeSpan.FromDays(30) 
                : TimeSpan.FromDays(3);
            return _jwtHelper.GenerateTokens(claims, refreshTokenLifetime);
        }

        public async Task<TokenDTO> RefreshAccessToken(User user)
        { if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await Login(user, false);
        }
    }
