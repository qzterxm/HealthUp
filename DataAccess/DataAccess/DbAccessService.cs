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

        Task<Guid> AddDoctorVisit(DoctorVisit visit);
        Task<List<DoctorVisit>> GetDoctorVisits(Guid userId);

        Task<UserFile?> GetUserFileById(Guid fileId, Guid userId);
        Task<int> DeleteUserFileById(Guid fileId, Guid userId);
        Task<List<UserFile>> GetUserFilesByUserId(Guid userId);


        Task<int> AddMedication(Medication medication);
        Task<List<Medication>> GetMedications(Guid userId);
        Task<int> UpdateMedication(Medication medication);
        Task<bool> DeleteMedication(Guid id);
        
        Task<int> AddSleep(SleepDTO sleepDto);
        Task<List<SleepDTO>> GetSleepRecords(Guid userId);
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
                return await connection.ExecuteAsync(sqlQuery, new { Id = id.ToString().ToUpper() });
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
              return await connection.QueryFirstOrDefaultAsync<TResult>(sqlQuery, new { Id = id.ToString().ToUpper() });
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
                var paramValue = value is Guid guidValue ? guidValue.ToString().ToUpper() : value;
                parameters.Add(parameterName, paramValue);

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
            using var connection = new SqliteConnection(_connectionString);
            var parameters = new
            {
                Id = measurementDto.Id.ToString().ToUpper(),
                UserId = measurementDto.UserId.ToString().ToUpper(),
                measurementDto.MeasuredAt,
                measurementDto.Systolic,
                measurementDto.Diastolic,
                measurementDto.HeartRate
            };
            return await connection.ExecuteAsync(SqlQueries.AddHealthMeasurement, parameters);
        }

        public async Task<List<HealthMeasurementDTO>> GetHealthMeasurements(Guid userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId.ToString().ToUpper());
            return await GetRecordsByParameters<HealthMeasurementDTO>(SqlQueries.GetMeasurements, parameters);
        }

    public async Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto)
{
    try
    {
        var userCheckSql = "SELECT COUNT(1) FROM Users WHERE Id = @UserId";
        using var checkConnection = new SqliteConnection(_connectionString);
        await checkConnection.OpenAsync();
        var userExists = await checkConnection.ExecuteScalarAsync<int>(userCheckSql, new { UserId = anthropometrydto.UserId.ToString().ToUpper() });
     
        if (userExists == 0)
        {
            throw new InvalidOperationException($"User with ID {anthropometrydto.UserId} not found");
        }

        int? ageToStore = anthropometrydto.Age;
        if (!ageToStore.HasValue)
        {
            var getUserAgeSql = "SELECT Age FROM Users WHERE Id = @UserId";
            var currentAge = await checkConnection.ExecuteScalarAsync<int?>(getUserAgeSql, new { UserId = anthropometrydto.UserId.ToString().ToUpper() });
            ageToStore = currentAge;
        }
        else
        {
          
            var updateUserAgeSql = "UPDATE Users SET Age = @Age WHERE Id = @UserId";
            await checkConnection.ExecuteAsync(updateUserAgeSql, new 
            { 
                UserId = anthropometrydto.UserId.ToString().ToUpper(),
                Age = ageToStore.Value
            });
        }

        var newId = Guid.NewGuid();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", newId.ToString().ToUpper());
        parameters.Add("@UserId", anthropometrydto.UserId.ToString().ToUpper());
        parameters.Add("@MeasuredAt", anthropometrydto.MeasuredAt);
        parameters.Add("@Weight", anthropometrydto.Weight);
        parameters.Add("@Height", anthropometrydto.Height);
        parameters.Add("@Sugar", anthropometrydto.Sugar);
        parameters.Add("@BloodType", anthropometrydto.BloodType.HasValue ? (int?)anthropometrydto.BloodType : null);
        parameters.Add("@Age", ageToStore); 

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
        var result = await connection.ExecuteAsync(SqlQueries.AddAnthropometry, parameters);
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
                
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId.ToString().ToUpper());
                var results = await GetRecordsByParameters<AnthropometryDTO>(SqlQueries.GetAnthropometries, parameters);
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
            using var connection = new SqliteConnection(_connectionString);
            var parameters = new
            {
                Id = code.Id.ToString().ToUpper(),
                UserId = code.UserId.ToString().ToUpper(),
                code.ResetCode,
                code.ExpiresAt,
                code.IsUsed
            };
            return await connection.ExecuteAsync(SqlQueries.AddPasswordResetCode, parameters);
        }

        public async Task<PasswordResetCode?> GetValidResetCode(Guid userId, int resetCode, DateTime currentTime)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId.ToString().ToUpper());
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
            
            using var connection = new SqliteConnection(_connectionString);
            var parameters = new
            {
                Id = note.Id.ToString().ToUpper(),
                UserId = note.UserId.ToString().ToUpper(),
                note.CreatedAt,
                note.NoteTitle,
                note.NoteText
            };
            return await connection.ExecuteAsync(SqlQueries.AddUserNote, parameters);
        }

        public async Task<List<UserNoteDTO>> GetUserNotes(Guid userId)
        {
            return await GetRecordsByParameters<UserNoteDTO>(SqlQueries.GetUserNotes,
                new DynamicParameters(new { UserId = userId.ToString().ToUpper() }));
        }

        public async Task<bool> DeleteUserNote(Guid userId, Guid noteId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId.ToString().ToUpper());
            parameters.Add("@Id", noteId.ToString().ToUpper());

            await using var connection = new SqliteConnection(_connectionString);
            var rows = await connection.ExecuteAsync(SqlQueries.DeleteUserNote, parameters);
            return rows > 0;
        }
        

        public async Task<Guid> AddDoctorVisit(DoctorVisit visit)
        {
            visit.Id = Guid.NewGuid();
            
            using var connection = new SqliteConnection(_connectionString);
            var parameters = new
            {
                Id = visit.Id.ToString().ToUpper(),
                UserId = visit.UserId.ToString().ToUpper(),
                visit.Specialist,
                visit.VisitType,
                visit.Diagnosis,
                visit.Prescription,
                visit.VisitedAt
            };
            await connection.ExecuteAsync(SqlQueries.AddDoctorVisit, parameters);
            return visit.Id;
        }

        public async Task<List<DoctorVisit>> GetDoctorVisits(Guid userId)
        {
            return await GetRecordsByParameters<DoctorVisit>(SqlQueries.GetDoctorVisits,
                new DynamicParameters(new { UserId = userId.ToString().ToUpper() }));
        }
        

        public async Task<UserFile?> GetUserFileById(Guid fileId, Guid userId)
        {
            await using var connection = new SqliteConnection(_connectionString);
            var parameters = new
            {
                Id = fileId.ToString().ToUpper(),
                UserId = userId.ToString().ToUpper()
            };
            
            const string sql = "SELECT * FROM UserFiles WHERE Id = @Id AND UserId = @UserId";
            return await connection.QuerySingleOrDefaultAsync<UserFile>(sql, parameters);
        }

        public async Task<int> DeleteUserFileById(Guid fileId, Guid userId)
        {
            await using var connection = new SqliteConnection(_connectionString);
            var parameters = new
            {
                Id = fileId.ToString().ToUpper(),
                UserId = userId.ToString().ToUpper()
            };
            
            const string sql = "DELETE FROM UserFiles WHERE Id = @Id AND UserId = @UserId";
            return await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<List<UserFile>> GetUserFilesByUserId(Guid userId)
        {
            try
            {
              
                await using var connection = new SqliteConnection(_connectionString);
                const string sql = "SELECT * FROM UserFiles WHERE UserId = @UserId ORDER BY UploadedAt DESC";
                var parameters = new { UserId = userId.ToString().ToUpper() };
                var results = await connection.QueryAsync<UserFile>(sql, parameters);
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user files for user {UserId}", userId);
                throw;
            }
        }
        

     public async Task<int> AddMedication(Medication medication)
{
    try
    {
        medication.Id = Guid.NewGuid();
        medication.CreatedAt = DateTime.UtcNow;
        medication.UpdatedAt = DateTime.UtcNow;

        if (medication.StartDate == default)
        {
            medication.StartDate = DateTime.Today;
        }
        
        if (!medication.EndDate.HasValue)
        {
            medication.EndDate = CalculateEndDate(medication.Duration);
        }
        
        medication.WeekDaysJson ??= "[]";

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            INSERT INTO Medications 
            (Id, UserId, NameOfMedication, Dose, TimesJson, WeekDaysJson, Type, Duration, StartDate, EndDate, CreatedAt, UpdatedAt) 
            VALUES 
            (@Id, @UserId, @NameOfMedication, @Dose, @TimesJson, @WeekDaysJson, @Type, @Duration, @StartDate, @EndDate, @CreatedAt, @UpdatedAt);";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", medication.Id.ToString().ToUpper());
        parameters.Add("@UserId", medication.UserId.ToString().ToUpper());
        parameters.Add("@NameOfMedication", medication.NameOfMedication);
        parameters.Add("@Dose", medication.Dose);
        parameters.Add("@TimesJson", medication.TimesJson ?? "[]");
        parameters.Add("@WeekDaysJson", medication.WeekDaysJson ?? "[]");
        parameters.Add("@Type", medication.Type);
        parameters.Add("@Duration", medication.Duration);
        parameters.Add("@StartDate", medication.StartDate);
        parameters.Add("@EndDate", medication.EndDate);
        parameters.Add("@CreatedAt", medication.CreatedAt);
        parameters.Add("@UpdatedAt", medication.UpdatedAt);

        _logger.LogInformation("Adding medication with ID: {MedicationId}", medication.Id);
        
        var result = await connection.ExecuteAsync(sql, parameters);
        
        _logger.LogInformation("Medication added successfully. Rows affected: {RowsAffected}", result);
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding medication for user {UserId}", medication.UserId);
        throw;
    }
}

public async Task<List<Medication>> GetMedications(Guid userId)
{
    try
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT * FROM Medications 
            WHERE UserId = @UserId 
            ORDER BY CreatedAt DESC";
            
        var parameters = new { UserId = userId.ToString().ToUpper() };

        var results = await connection.QueryAsync<Medication>(query, parameters);
        
        _logger.LogInformation("Found {Count} medications for user {UserId}", results.Count(), userId);
        
        return results.ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting medications for user {UserId}", userId);
        throw;
    }
}

public async Task<int> UpdateMedication(Medication medication)
{
    try
    {
        medication.UpdatedAt = DateTime.UtcNow;

      
        if (!medication.EndDate.HasValue)
        {
            medication.EndDate = CalculateEndDate(medication.Duration);
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            UPDATE Medications SET
                NameOfMedication = @NameOfMedication,
                Dose = @Dose,
                TimesJson = @TimesJson,
                WeekDaysJson = @WeekDaysJson,
                Type = @Type,
                Duration = @Duration,
                StartDate = @StartDate,
                EndDate = @EndDate,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND UserId = @UserId;";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", medication.Id.ToString().ToUpper());
        parameters.Add("@UserId", medication.UserId.ToString().ToUpper());
        parameters.Add("@NameOfMedication", medication.NameOfMedication);
        parameters.Add("@Dose", medication.Dose);
        parameters.Add("@TimesJson", medication.TimesJson ?? "[]");
        parameters.Add("@WeekDaysJson", medication.WeekDaysJson ?? "[]");
        parameters.Add("@Type", medication.Type);
        parameters.Add("@Duration", medication.Duration);
        parameters.Add("@StartDate", medication.StartDate);
        parameters.Add("@EndDate", medication.EndDate);
        parameters.Add("@UpdatedAt", medication.UpdatedAt);
        
        _logger.LogInformation("Updating medication with ID: {MedicationId}", medication.Id);
        
        var result = await connection.ExecuteAsync(sql, parameters);
        
        _logger.LogInformation("Medication updated successfully. Rows affected: {RowsAffected}", result);
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating medication {MedicationId}", medication.Id);
        throw;
    }
}

private DateTime CalculateEndDate(string duration)
{
    var startDate = DateTime.Today;
    return duration?.ToLower() switch
    {
        "1 week" => startDate.AddDays(7),
        "2 weeks" => startDate.AddDays(14),
        "1 month" => startDate.AddMonths(1),
        "3 months" => startDate.AddMonths(3),
        "6 months" => startDate.AddMonths(6),
        "1 year" => startDate.AddYears(1),
        "indefinite" => startDate.AddYears(10), 
        _ => startDate.AddMonths(1) 
    };
}

        public async Task<bool> DeleteMedication(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "DELETE FROM Medications WHERE Id = @Id";
            var parameters = new { Id = id.ToString().ToUpper() };

            int affected = await connection.ExecuteAsync(query, parameters);
            return affected > 0;
        }
   
        
        public async Task<int> AddSleep(SleepDTO sleepDto)
        {
            if (sleepDto.Id == Guid.Empty)
            {
                sleepDto.Id = Guid.NewGuid();
            }

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", sleepDto.Id.ToString().ToUpper());
            parameters.Add("@UserId", sleepDto.UserId.ToString().ToUpper());
            parameters.Add("@Date", sleepDto.Date);
            parameters.Add("@StartTime", sleepDto.StartTime);
            parameters.Add("@EndTime", sleepDto.EndTime);
            parameters.Add("@TotalDurationMinutes", sleepDto.TotalDurationMinutes);
            parameters.Add("@SleepScore", sleepDto.SleepScore);
            parameters.Add("@SleepStatus", sleepDto.SleepStatus);

            return await connection.ExecuteAsync(SqlQueries.AddSleep, parameters);
        }
        public async Task<List<SleepDTO>> GetSleepRecords(Guid userId)
        {
            try 
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new { UserId = userId.ToString().ToUpper() };
                var result = await connection.QueryAsync<SleepDTO>(SqlQueries.GetSleepByUserId, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sleep records for user {UserId}", userId);
                throw;
            }
        }
    }
} 