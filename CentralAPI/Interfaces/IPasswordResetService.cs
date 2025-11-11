namespace WebApplication1.Interfaces;

public interface IPasswordResetService
{
    Task<bool> SendPasswordResetCode(string email);
    Task<Guid?> ValidateResetCode(Guid userId, int resetCode);
    Task<bool> CompletePasswordReset(Guid userId, string hashedPassword, int resetCode);
    Task MarkResetCodeAsUsed(Guid userId, int resetCode);
}