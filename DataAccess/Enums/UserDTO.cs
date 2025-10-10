namespace DataAccess.Enums;

public class UserDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public UserRoles UserRole { get; set; }
}