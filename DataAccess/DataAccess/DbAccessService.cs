using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using Dapper;
using DataAccess.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataAccess.DataAccess
{
    public interface IDbAccessService
    {
        string? GetConnectionString();
        Task<List<TResult>> GetRecords<TResult>(string sqlQuery);
        Task<int> AddRecord<TEntity>(string sqlQuery, TEntity entity);
        Task<int> UpdateRecord<TEntity>(string sqlQuery, TEntity entity);
        Task<int> DeleteRecordById(string sqlQuery, Guid id);
        Task<TResult?> GetRecordById<TResult>(string sqlQuery, Guid id);
        Task<TResult?> GetOneByParameter<TResult>(string sqlQuery, string parameterName, object value);
        Task<List<TResult>> GetRecordsByParameters<TResult>(string sqlQuery, DynamicParameters parameters);

        Task<int> AddHealthMeasurement(HealthMeasurementDTO measurementDto);
        Task<List<HealthMeasurementDTO>> GetHealthMeasurements(Guid userId);

        Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto);
        Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId);

        Task<int> AddPasswordResetCode(PasswordResetCode code);
        Task<PasswordResetCode?> GetValidResetCode(Guid userId, int resetCode, DateTime currentTime); 

        Task<int> AddUserFile(UserFile file);
        Task<int> DeleteUserFile(Guid fileId);
        Task<UserFile?> GetUserFile(Guid fileId);
        
        Task<int> AddUserNote(UserNoteDTO note);
        Task<List<UserNoteDTO>> GetUserNotes(Guid userId);
        Task<bool> DeleteUserNote(Guid userId, Guid noteId);

        Task<Guid> AddDoctorVisit(DoctorVisitDTO visit);
        Task<List<DoctorVisitDTO>> GetDoctorVisits(Guid userId);
        
        Task<UserFile?> GetUserFileById(Guid fileId, Guid userId);
        Task<int> DeleteUserFileById(Guid fileId, Guid userId);
    }

      public class DbAccessService : IDbAccessService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<DbAccessService> _logger;

        public DbAccessService(IConfiguration configuration, ILogger<DbAccessService> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException(
                                    "Connection string 'DefaultConnection' not found.");

            _logger.LogInformation($"Database connection: {_connectionString}");
        }

        public string? GetConnectionString() => _connectionString;

        public async Task<List<TResult>> GetRecords<TResult>(string sqlQuery)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.QueryAsync<TResult>(sqlQuery);
                return result.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetRecords");
                throw new InvalidOperationException($"Error retrieving records using query '{sqlQuery}': {e.Message}");
            }
        }

        public async Task<List<TResult>> GetRecordsByParameters<TResult>(string sqlQuery, DynamicParameters parameters)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.QueryAsync<TResult>(sqlQuery, parameters);
                return result.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving records using query '{sqlQuery}' with parameters");
                throw new InvalidOperationException(
                    $"Error retrieving records using query '{sqlQuery}' with parameters: {e.Message}");
            }
        }

        public async Task<int> AddRecord<TEntity>(string sqlQuery, TEntity entity)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sqlQuery, entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding record");
                throw new InvalidOperationException($"Error adding record using query '{sqlQuery}': {e.Message}");
            }
        }

        public async Task<int> UpdateRecord<TEntity>(string sqlQuery, TEntity entity)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sqlQuery, entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating record");
                throw new InvalidOperationException($"Error updating record using query '{sqlQuery}': {e.Message}");
            }
        }

        public async Task<int> DeleteRecordById(string sqlQuery, Guid id)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sqlQuery, new { Id = id });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting record with ID {Id}", id);
                throw new InvalidOperationException(
                    $"Error deleting record with ID '{id}' using query '{sqlQuery}': {e.Message}");
            }
        }

        public async Task<TResult?> GetRecordById<TResult>(string sqlQuery, Guid id)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<TResult>(sqlQuery, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving record with ID {Id}", id);
                throw;
            }
        }

        public async Task<TResult?> GetOneByParameter<TResult>(string sqlQuery, string parameterName, object value)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add(parameterName, value);

                return await connection.QueryFirstOrDefaultAsync<TResult>(sqlQuery, parameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetOneByParameter for {ParameterName} with value {Value}", parameterName,
                    value);
                throw new InvalidOperationException(
                    $"Error retrieving record with parameter '{parameterName}' and value '{value}' using query '{sqlQuery}': {e.Message}");
            }
        }

        

        public async Task<int> AddHealthMeasurement(HealthMeasurementDTO measurementDto)
        {
            measurementDto.Id ??= Guid.NewGuid();
            return await AddRecord(SqlQueries.AddHealthMeasurement, measurementDto);
        }

        public async Task<List<HealthMeasurementDTO>> GetHealthMeasurements(Guid userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await GetRecordsByParameters<HealthMeasurementDTO>(SqlQueries.GetMeasurements, parameters);
        }

        public async Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto)
        {
            try
            {
                _logger.LogInformation("=== Starting AddAnthrometry ===");
                _logger.LogInformation("User ID: {UserId}", anthropometrydto.UserId);

                var userCheckSql = "SELECT COUNT(1) FROM Users WHERE Id = @UserId";
                using var checkConnection = new SqliteConnection(_connectionString);
                await checkConnection.OpenAsync();
                
                var userExists = await checkConnection.ExecuteScalarAsync<int>(userCheckSql, new { UserId = anthropometrydto.UserId });
                _logger.LogInformation("User exists check: {UserExists}", userExists);

                if (userExists == 0)
                {
                    _logger.LogError("User with ID {UserId} does not exist in database", anthropometrydto.UserId);
                    throw new InvalidOperationException($"User with ID {anthropometrydto.UserId} not found");
                }

                var newId = Guid.NewGuid();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", newId);
                parameters.Add("@UserId", anthropometrydto.UserId);
                parameters.Add("@MeasuredAt", anthropometrydto.MeasuredAt);
                parameters.Add("@Weight", anthropometrydto.Weight);
                parameters.Add("@Height", anthropometrydto.Height);
                parameters.Add("@Sugar", anthropometrydto.Sugar);
                parameters.Add("@BloodType", anthropometrydto.BloodType.HasValue ? (int?)anthropometrydto.BloodType : null);

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
                
                var result = await connection.ExecuteAsync(SqlQueries.AddAnthropometry, parameters);
                
                _logger.LogInformation("AddAnthrometry completed successfully. Rows affected: {Result}", result);
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
            try
            {
                _logger.LogInformation("=== GetAnthropometries START ===");
                _logger.LogInformation("Querying anthropometry for user: {UserId}", userId);
        
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
        
                var results = await GetRecordsByParameters<AnthropometryDTO>(SqlQueries.GetAnthropometries, parameters);
        
                _logger.LogInformation("Raw SQL results count: {Count}", results.Count);
        
                foreach (var result in results)
                {
                    _logger.LogInformation("Anthro Record - UserId: {UserId}, Weight: {Weight}, Height: {Height}, Sugar: {Sugar}, BloodType: {BloodType}, MeasuredAt: {MeasuredAt}", 
                        result.UserId, result.Weight, result.Height, result.Sugar, result.BloodType, result.MeasuredAt);
                }
        
                _logger.LogInformation("=== GetAnthropometries END ===");
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting anthropometries for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> AddPasswordResetCode(PasswordResetCode code)
        {
            return await AddRecord(SqlQueries.AddPasswordResetCode, code);
        }

        public async Task<PasswordResetCode?> GetValidResetCode(Guid userId, int resetCode, DateTime currentTime)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@ResetCode", resetCode);
            parameters.Add("@CurrentTime", currentTime);
            var results =
                await GetRecordsByParameters<PasswordResetCode>(SqlQueries.GetValidPasswordResetCode, parameters);
            return results.FirstOrDefault();
        }

        public async Task<int> AddUserFile(UserFile file)
        {
            return await AddRecord(SqlQueries.AddUserFile, file);
        }

        public async Task<UserFile?> GetUserFile(Guid fileId)
        {
            return await GetRecordById<UserFile>(SqlQueries.GetUserFile, fileId);
        }

        public async Task<int> DeleteUserFile(Guid fileId)
        {
            return await DeleteRecordById(SqlQueries.DeleteUserFile, fileId);
        }

        public async Task<int> AddUserNote(UserNoteDTO note)
        {
            note.Id = Guid.NewGuid(); 
            note.CreatedAt = DateTime.UtcNow; 
            return await AddRecord(SqlQueries.AddUserNote, note);
        }

        public async Task<List<UserNoteDTO>> GetUserNotes(Guid userId)
        {
            return await GetRecordsByParameters<UserNoteDTO>(SqlQueries.GetUserNotes,
                new DynamicParameters(new { UserId = userId.ToString() }));
        }

        public async Task<bool> DeleteUserNote(Guid userId, Guid noteId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId.ToString());
            parameters.Add("@Id", noteId.ToString());

            await using var connection = new SqliteConnection(_connectionString);
            var rows = await connection.ExecuteAsync(SqlQueries.DeleteUserNote, parameters);
            return rows > 0;
        }

        public async Task<Guid> AddDoctorVisit(DoctorVisitDTO visit)
        {
            visit.Id = Guid.NewGuid();
            await AddRecord(SqlQueries.AddDoctorVisit, visit);
            return visit.Id;
        }

        public async Task<List<DoctorVisitDTO>> GetDoctorVisits(Guid userId)
        {
            return await GetRecordsByParameters<DoctorVisitDTO>(SqlQueries.GetDoctorVisits,
                new DynamicParameters(new { UserId = userId.ToString() }));
        }
        

        public async Task<UserFile?> GetUserFileById(Guid fileId, Guid userId)
        {
            await using var connection = new SqliteConnection(_connectionString);
            const string sql = "SELECT * FROM UserFiles WHERE Id = @Id AND UserId = @UserId";
            return await connection.QuerySingleOrDefaultAsync<UserFile>(sql,
                new { Id = fileId.ToString(), UserId = userId });
        }

        public async Task<int> DeleteUserFileById(Guid fileId, Guid userId)
        {
            await using var connection = new SqliteConnection(_connectionString);
            const string sql = "DELETE FROM UserFiles WHERE Id = @Id AND UserId = @UserId";
            return await connection.ExecuteAsync(sql, new { Id = fileId.ToString(), UserId = userId.ToString() });
        }
    }
}
