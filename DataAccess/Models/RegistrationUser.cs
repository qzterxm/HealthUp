using WebApplication1.Enums;

namespace DataAccess.Models;

public class RegistrationUser
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public UserRoles Role { get; set; } = UserRoles.User;
}