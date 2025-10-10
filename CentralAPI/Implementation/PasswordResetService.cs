using DataAccess.DataAccess;
using DataAccess.Models;
using WebApplication1.Interfaces;
using WebApplication1.EmailSender;

namespace WebApplication1.Implementation
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IDbAccessService _dbAccessService;
        private readonly IUserService _userService;
        private readonly UseEmailSender _emailSender;
        private readonly ILogger<PasswordResetService>  _logger;

        public PasswordResetService(IDbAccessService dbAccessService, IUserService userService, UseEmailSender emailSender,  ILogger<PasswordResetService> logger)
        {
            _dbAccessService = dbAccessService;
            _userService = userService;
            _emailSender = emailSender;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendPasswordResetCode(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null) 
            {
                _logger.LogError($"User with email {email} not found");
               return false;
            }

            var resetCode = new Random().Next(1000, 9999);
            var expiration = DateTime.UtcNow.AddMinutes(15);

            var entity = new PasswordResetCode
            {
                UserId = user.Id,
                ResetCode = resetCode,
                ExpiresAt = expiration,
                IsUsed = false
            };

            await _dbAccessService.AddPasswordResetCode(entity);

            await _emailSender.SendPasswordResetLink(email, resetCode.ToString());
            _logger.LogInformation("Password reset code {ResetCode} generated for User {UserId} ({Email}) at {Time}", 
                resetCode, user.Id, email, DateTime.UtcNow);
            return true;
        }

        public async Task<Guid?> ValidateResetCode(Guid userId, int resetCode)
        {
            var entity = await _dbAccessService.GetValidResetCode(userId, resetCode);
            return entity?.UserId;
        }

        
        public async Task<bool> CompletePasswordReset(Guid userId, string newPassword, int resetCode)
        {
            var user = await _userService.GetById(userId);
            if (user == null) return false;

            // TODO: валідація паролю та добавити процедуру
            user.Password = newPassword;
            await _userService.UpdateUser(userId, user);

    
            var code = await _dbAccessService.GetValidResetCode(userId, resetCode);
            if (code != null)
            {
                code.IsUsed = true;
                await _dbAccessService.UpdateRecord("sp_Users_UpdatePasswordResetCode", code);
            }

            return true;
        }
    }
}
