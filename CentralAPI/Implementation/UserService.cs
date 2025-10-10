using DataAccess.Enums;
using DataAccess.Interfaces;
using DataAccess.Models;
using WebApplication1.Interfaces;

namespace WebApplication1.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger <UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger <UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> GetById(Guid id)
    {
        return await _userRepository.GetById(id);
    }
    public async Task<bool> CreateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.UserName))
            return false;

        var existingUser = await _userRepository.GetUserByEmail(user.Email);
        if (existingUser != null)
            return false;

        user.Id = Guid.NewGuid();
        return await _userRepository.CreateUser(user);
    }
    public Task<User?> GetUserByEmail(string email)
    {
        return _userRepository.GetUserByEmail(email);
    }
    public async Task<bool> UpdateUser(Guid id, User user)
    {
        if (user is null || id == Guid.Empty) return false;
            
        var existingUser = await _userRepository.GetById(id);
        if (existingUser == null) return false;
            

        existingUser.Email = user.Email;
        existingUser.UserName = user.UserName;
        existingUser.UserRole = user.UserRole;

        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            existingUser.Password = user.Password;
        }
        return await _userRepository.UpdateUser(id, existingUser);
    }
    public async Task<bool> ChangeUserRole(Guid id, UserRoles newRole)
    {
        var user = await _userRepository.GetById(id);
        if (user is null)
        {
            return false;
        }
        user.UserRole = newRole;
        var updateResult = await _userRepository.UpdateUser(id, user);

        if (!updateResult)
            return false;

        return true; 
    }
    public Task<List<User>> GetAllUsers()
         {
             return  _userRepository.GetAllUsers();
         }
    public Task<bool> DeleteUser(Guid id)
    {
        return _userRepository.DeleteUser(id);
    }
}