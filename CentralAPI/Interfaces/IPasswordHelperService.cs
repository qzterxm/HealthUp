namespace WebApplication1.Interfaces;

public interface IPasswordHelperService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}