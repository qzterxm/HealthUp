using System.Collections.Concurrent;
using DataAccess.DataAccess;
using DataAccess.Enums;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using WebApplication1.Enums;

namespace DataAccess.Implementation;

public class UserRepository : IUserRepository
{


    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly IUserRepository _usersRepository;
    private readonly IDbAccessService _dbAccessService;
   

    public async Task<User?> GetByEmail(string email)
    {
        try
        {
            return _users.Values.FirstOrDefault(x => 
                string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    

    public async Task<bool> ChangeUserRoleAsync(Guid id, UserRoles newRole)
    {
        try
        {
            var userToUpdate = await GetById(id);
            if (userToUpdate == null)
                return false;
            
            userToUpdate.UserRole = newRole;
            var userUpdateResult = await _dbAccessService.UpdateRecord<User>(StoredProceduresNames.UpdateUser, userToUpdate);
            return userUpdateResult > 0;
        }
        catch (Exception ex)
        {
          
            return false;
        }
    }

    public async Task<User?> GetById(Guid id)
    {
        try
        {
            var user = await _dbAccessService.GetOneByParameter<User>(StoredProceduresNames.GetUserById, "Id", id);
            return user;
        }
        catch(Exception ex)
        {
            return null;
        }
    }

    public async Task<bool> UpdateUser(Guid id, User entity)
    {
        try
        {
            var userToUpdate = await GetById(id);
            if (userToUpdate == null)
                return false;

            userToUpdate.UserRole = entity.UserRole;
            userToUpdate.UserName = entity.UserName;
            userToUpdate.Email = entity.Email;
            userToUpdate.Password = entity.Password;

            var userUpdateResult = await _dbAccessService.UpdateRecord<User>(StoredProceduresNames.UpdateUser, userToUpdate);
            return userUpdateResult > 0;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> CreateUser(User entity)
    {
            try
            {
                var userAddResult = await _dbAccessService.AddRecord<User>(StoredProceduresNames.CreateUser, entity);
                return userAddResult > 0 ? true : false;
            }
            catch (Exception ex)
            {
               
                return false;
            }
        
    }
}

