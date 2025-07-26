using DataAccess.Enums;
using DataAccess.Interfaces;
using WebApplication1.Enums;
using WebApplication1.Interfaces;

namespace WebApplication1.Implementation;

public class UserService :IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CreateUser(User user)
    { 
        if (user is null) return false;
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
                return false;
            return await _userRepository.CreateUser(user);
    }

    public Task<User?> GetUserByEmail(string email)
    {
        return _userService.GetUserByEmail(email);
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


    public async Task<bool> UpdateUser(Guid id, User user)
    {
        if (user is null || id == Guid.Empty) return false;
            
        var existingUser = await _userRepository.GetById(id);
        if (existingUser == null) return false;
            

        existingUser.Email = user.Email;
        existingUser.UserName = user.UserName;
        existingUser.UserRole = user.UserRole;

        return await _userRepository.UpdateUser(id, existingUser);
    }

 
}