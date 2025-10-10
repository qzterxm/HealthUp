
using DataAccess.Enums;
using DataAccess.Models;

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
    Task AddMeasurement(HealthMeasurementDTO measurementDto);
    Task<List<HealthMeasurementDTO>> GetMeasurements(Guid userId);
    Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto);
    Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId);
}