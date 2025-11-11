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
            var user = await _dbAccessService.GetOneByParameter<User>(SqlQueries.GetUserById, "Id", id);
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
                SqlQueries.CreateUser,
                entity 
            );
     
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
                await _dbAccessService.GetOneByParameter<User>(SqlQueries.GetUserByEmail, "Email", email);
            
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
                await _dbAccessService.UpdateRecord<User>(SqlQueries.UpdateUser, userToUpdate);
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
            _logger.LogInformation("Updating user: {UserId}", id);

            var userToUpdate = await GetById(id);
            if (userToUpdate == null)
            {
                _logger.LogWarning("User not found for update: {UserId}", id);
                return false;
            }

            
            userToUpdate.UserRole = entity.UserRole;
            userToUpdate.UserName = entity.UserName;
            userToUpdate.Email = entity.Email;
            userToUpdate.Password = entity.Password; 
            userToUpdate.Gender = entity.Gender;
            userToUpdate.Age = entity.Age;
            userToUpdate.DateOfBirth = entity.DateOfBirth;
            userToUpdate.Country = entity.Country;
            userToUpdate.PhoneNumber = entity.PhoneNumber;

            _logger.LogInformation("Password in update: {Password}", userToUpdate.Password);

            var userUpdateResult = await _dbAccessService.UpdateRecord<User>(SqlQueries.UpdateUser, userToUpdate);
        
            _logger.LogInformation("Update record result: {Result}", userUpdateResult);
        
            return userUpdateResult > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return false;
        }
    }
    
    
    public async Task<List<User>> GetAllUsers()
    {
        try
        {
            var user = await _dbAccessService.GetRecords<User>(SqlQueries.GetAllUsers);
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
                var user = await _dbAccessService.DeleteRecordById(SqlQueries.DeleteUser,  id);
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
        try
        {
            _logger.LogInformation("Starting AddAnthrometry for user {UserId}", anthropometrydto.UserId);
        
            var result = await _dbAccessService.AddAnthrometry(anthropometrydto);
        
            _logger.LogInformation("AddAnthrometry completed with result: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddAnthrometry for user {UserId}", anthropometrydto.UserId);
            throw;
        }
    }

    public async Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId)
    {
        return await _dbAccessService.GetAnthropometries(userId);
    }
    
    
    public async Task<HealthMeasurementDTO?> GetLatestMeasurement(Guid userId)
    {
        try
        {
            var measurements = await GetMeasurements(userId);
            return measurements.OrderByDescending(m => m.MeasuredAt).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetLatestMeasurement] Exception occurred for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<AnthropometryDTO?> GetLatestAnthropometry(Guid userId)
    {
        try
        {
            var anthropometries = await GetAnthropometries(userId);
        
            _logger.LogInformation($"Found {anthropometries.Count} anthropometry records for user {userId}");
            foreach (var anthro in anthropometries.OrderByDescending(a => a.MeasuredAt))
            {
                _logger.LogInformation($"Record: MeasuredAt={anthro.MeasuredAt}, Weight={anthro.Weight}, Height={anthro.Height}, Sugar={anthro.Sugar}, BloodType={anthro.BloodType}");
            }
        
            var latest = anthropometries.OrderByDescending(a => a.MeasuredAt).FirstOrDefault();
            _logger.LogInformation($"Selected latest: MeasuredAt={latest?.MeasuredAt}, Sugar={latest?.Sugar}");
        
            return latest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetLatestAnthropometry] Exception occurred for user: {UserId}", userId);
            return null;
        }
    }
    public async Task<bool> UpdateUserHealthData(UpdateUserHealthDataDTO healthData)
{
    try
    {
        _logger.LogInformation("Updating health data for user: {UserId}", healthData.UserId);

        
        var existingUser = await GetById(healthData.UserId);
        if (existingUser == null)
        {
            _logger.LogWarning("User not found: {UserId}", healthData.UserId);
            return false;
        }

        existingUser.Age = healthData.Age;
        existingUser.Gender = healthData.Gender;
        existingUser.DateOfBirth = healthData.DateOfBirth;
        existingUser.Country = healthData.Country ?? existingUser.Country;
        existingUser.PhoneNumber = healthData.PhoneNumber ?? existingUser.PhoneNumber;

        
        var updateResult = await _dbAccessService.UpdateRecord<User>(SqlQueries.UpdateUser, existingUser);
        
        if (updateResult > 0 && (healthData.Height.HasValue || healthData.Weight.HasValue || healthData.SugarLevel.HasValue || healthData.BloodType.HasValue))
        {
          
            var anthropometryDto = new AnthropometryDTO
            {
                UserId = healthData.UserId,
                MeasuredAt = DateTime.UtcNow,
                Height = healthData.Height ?? 0,
                Weight = healthData.Weight ?? 0,
                Sugar = healthData.SugarLevel ?? 0,
                BloodType = healthData.BloodType
            };

            await AddAnthrometry(anthropometryDto);
            _logger.LogInformation("Health data and anthropometry updated for user: {UserId}", healthData.UserId);
        }
        else
        {
            _logger.LogInformation("User data updated for user: {UserId}", healthData.UserId);
        }

        return updateResult > 0;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating health data for user: {UserId}", healthData.UserId);
        return false;
    }
}
}

