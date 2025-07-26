
using DataAccess.Enums;
using WebApplication1.Enums;

namespace DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email);
    Task<bool> ChangeUserRoleAsync(Guid id, UserRoles newRole);
    Task<User?> GetById(Guid id);
    Task<bool> UpdateUser(Guid id, User entity);
    Task<bool> CreateUser(User user);

}