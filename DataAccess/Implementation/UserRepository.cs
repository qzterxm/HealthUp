using System.Collections.Concurrent;
using DataAccess.DataAccess;
using DataAccess.Enums;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataAccess.Implementation;

public class UserRepository : IUserRepository
{


    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly IUserRepository _usersRepository;
    private readonly IDbAccessService _dbAccessService;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IConfiguration configuration,
        IDbAccessService dbAccessService,
        ILogger<UserRepository> logger)
    {
        _configuration = configuration;
        _dbAccessService = dbAccessService;
        _logger = logger;
    }
    public async  Task<User?> GetById(Guid id)
    {
        try
        {
            var user = await _dbAccessService.GetOneByParameter<User>(StoredProceduresNames.GetUserById, "Id", id);
            return user;
        }
        catch (Exception ex)
        {
            return null;
        } 
    }
    public async Task<bool> CreateUser(User entity)
         {
             try
             {
                 var rows = await _dbAccessService.AddRecord(
                     StoredProceduresNames.CreateUser,
                     new
                     {
                         entity.Id,
                         entity.Email,
                         entity.UserName,
                         entity.Password,
                         UserRole = (int)entity.UserRole
                     });
     
                 if (rows == 0)
                 {
                     _logger.LogWarning("[CreateUser] No rows affected. Possible duplicate email: {Email}", entity.Email);
                 }
     
                 return rows > 0;
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "[CreateUser] Exception occurred while creating user with Email: {Email}", entity.Email);
                 return false;
             }
         }
    public async Task<User?> GetUserByEmail(string email)
    {
        try
        {
            var userAddResult =
                await _dbAccessService.GetOneByParameter<User>(StoredProceduresNames.GetUserByEmail, "Email", email);
            
            return userAddResult;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<bool> ChangeUserRole(Guid id, UserRoles newRole)
    {
        try
        {
            var userToUpdate = await GetById(id);
            if (userToUpdate == null)
                return false;

            userToUpdate.UserRole = newRole;
            var userUpdateResult =
                await _dbAccessService.UpdateRecord<User>(StoredProceduresNames.UpdateUser, userToUpdate);
            return userUpdateResult > 0;
        }
        catch (Exception ex)
        {

            return false;
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

            var userUpdateResult =
                await _dbAccessService.UpdateRecord<User>(StoredProceduresNames.UpdateUser, userToUpdate);
            return userUpdateResult > 0;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public async Task<List<User>> GetAllUsers()
    {
        try
        {
            var user = await _dbAccessService.GetRecords<User>(StoredProceduresNames.GetAllUsers);
            return user;
        }
        catch (Exception ex)
        {
            return new List<User>();
        }
    }
    public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                var user = await _dbAccessService.DeleteRecordById(StoredProceduresNames.DeleteUser,  id);
                return true;
            }
            catch(Exception ex)
            {
             return false;   
            }
        }

    public async Task AddMeasurement(HealthMeasurementDTO measurementDto)
    {
        await _dbAccessService.AddHealthMeasurement(measurementDto);
    }

    public async Task<List<HealthMeasurementDTO>> GetMeasurements(Guid userId)
    {
        return await _dbAccessService.GetHealthMeasurements(userId);
    }

    public async Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto)
    {
        return await _dbAccessService.AddAnthrometry(anthropometrydto);
    }

    public async Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId)
    {
        return await _dbAccessService.GetAnthropometries(userId);
    }
    
}

