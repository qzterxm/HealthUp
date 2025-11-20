
using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetById(Guid id);
    Task<bool> CreateUser(User user);
    Task<User?> GetUserByEmail(string email);
    Task<bool> UpdateUser(Guid id, User entity);
    Task<List<User>> GetAllUsers();
    Task<bool> DeleteUser(Guid id);
    Task AddMeasurement(HealthMeasurementDTO measurementDto);
    Task<List<HealthMeasurementDTO>> GetMeasurements(Guid userId);
    Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto);
    Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId);
    Task<HealthMeasurementDTO?> GetLatestMeasurement(Guid userId); 
    Task<AnthropometryDTO?> GetLatestAnthropometry(Guid userId);
    Task<bool> UpdateUserHealthData(UpdateUserHealthDataDTO healthData);
    
    
    Task<int> AddMedication (Medication medication);
    Task<List<Medication>> GetMedications(Guid userId);
    Task<bool> DeleteMedication(Guid id);
    
    Task<int> AddSleepData(SleepDTO sleepDto);
    Task<List<SleepDTO>> GetSleepData(Guid userId);
}