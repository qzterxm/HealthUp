namespace WebApplication1.Interfaces;

public interface IPasswordResetService
{ 
    Task<bool> SendPasswordResetCode(string email);
    Task<bool> CompletePasswordReset(Guid userId, string newPassword, int resetCode);
    Task<Guid?> ValidateResetCode(Guid userId,int resetCode);
}