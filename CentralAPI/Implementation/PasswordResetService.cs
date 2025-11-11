using DataAccess.DataAccess;
using DataAccess.Models;
using DataAccess.Interfaces; 
using WebApplication1.Interfaces;
using WebApplication1.EmailSender;

namespace WebApplication1.Implementation
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IDbAccessService _dbAccessService;
        private readonly IUserRepository _userRepository; 
        private readonly UseEmailSender _emailSender;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            IDbAccessService dbAccessService, 
            IUserRepository userRepository, 
            UseEmailSender emailSender,  
            ILogger<PasswordResetService> logger)
        {
            _dbAccessService = dbAccessService;
            _userRepository = userRepository; 
            _emailSender = emailSender;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendPasswordResetCode(string email)
        {
            var user = await _userRepository.GetUserByEmail(email); 
            if (user == null) 
            {
                _logger.LogError($"User with email {email} not found");
                return false;
            }

            var resetCode = new Random().Next(1000, 9999);
            var expiration = DateTime.UtcNow.AddMinutes(15);

            var entity = new PasswordResetCode
            {
                Id = Guid.NewGuid(), 
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
            var entity = await _dbAccessService.GetValidResetCode(userId, resetCode, DateTime.UtcNow);
            return entity?.UserId;
        }

        public async Task<bool> CompletePasswordReset(Guid userId, string hashedPassword, int resetCode)
        {
            try
            {
                _logger.LogInformation("Completing password reset for user: {UserId}", userId);
             
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    _logger.LogError("User not found: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("User before update - Email: {Email}, Current Hash: {CurrentHash}", 
                    user.Email, user.Password);

                user.Password = hashedPassword;
                
                var updateResult = await _userRepository.UpdateUser(userId, user);
                
                _logger.LogInformation("Update result: {Result}", updateResult);

                if (updateResult)
                {
                    
                    await MarkResetCodeAsUsed(userId, resetCode);
                    _logger.LogInformation("Password updated successfully for user: {UserId}", userId);
                    
                  
                    var updatedUser = await _userRepository.GetById(userId);
                    _logger.LogInformation("User after update - New Hash: {NewHash}", updatedUser?.Password);
                }

                return updateResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing password reset for user: {UserId}", userId);
                return false;
            }
        }

        public async Task MarkResetCodeAsUsed(Guid userId, int resetCode)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var validCode = await _dbAccessService.GetValidResetCode(userId, resetCode, currentTime);
                
                if (validCode != null)
                {
                    validCode.IsUsed = true;
                    
                    await _dbAccessService.UpdateRecord<PasswordResetCode>(
                        "UPDATE PasswordResetCodes SET IsUsed = 1 WHERE Id = @Id", 
                        validCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking reset code as used for user: {UserId}", userId);
            }
        }
    }
}