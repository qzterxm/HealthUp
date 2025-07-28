
using DataAccess.Enums;
using WebApplication1.Enums;

namespace DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetById(Guid id);
    Task<bool> CreateUser(User user);
    Task<User?> GetUserByEmail(string email);
    Task<bool> ChangeUserRole(Guid id, UserRoles newRole);
    Task<bool> UpdateUser(Guid id, User entity);
    Task<List<User>> GetAllUsers();
    Task<bool> DeleteUser(Guid id);

}