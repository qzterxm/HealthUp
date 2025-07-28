using DataAccess.Enums;
using WebApplication1.Enums;

namespace WebApplication1.Interfaces;

public interface IUserService
{
    Task<User?> GetById(Guid id);
    Task<bool> CreateUser(User user);
    Task<User?> GetUserByEmail(string email);
    Task<bool>UpdateUser(Guid id,User user); 
    Task<List<User>> GetAllUsers();
    Task<bool> ChangeUserRole(Guid id, UserRoles newRole);
    Task<bool> DeleteUser(Guid id);
    
    
}