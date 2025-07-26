
using WebApplication1.Enums;

namespace DataAccess.Enums;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Age { get; set; }
    public UserRoles UserRole { get; set; }
   
}