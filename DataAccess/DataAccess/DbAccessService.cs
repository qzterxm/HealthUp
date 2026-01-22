using Dapper;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql; // <--- ВАЖЛИВО!
using System.Data;

namespace DataAccess.DataAccess
{
    public interface IDbAccessService
    {
        string? GetConnectionString();
        Task InitDatabase();
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
        
        // Методи для ліків
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
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            _logger.LogInformation($"Database connection initialized.");
        }

        public string? GetConnectionString() => _connectionString;

        // --- УНІВЕРСАЛЬНИЙ МЕТОД ПІДКЛЮЧЕННЯ ---
        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task InitDatabase()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            await connection.ExecuteAsync(SqlQueries.UserDbSchema);
        }

        public async Task<List<TResult>> GetRecords<TResult>(string sqlQuery)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<TResult>(sqlQuery);
                return result.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetRecords");
                throw;
            }
        }

        public async Task<List<TResult>> GetRecordsByParameters<TResult>(string sqlQuery, DynamicParameters parameters)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<TResult>(sqlQuery, parameters);
                return result.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting records with parameters");
                throw;
            }
        }

        public async Task<int> AddRecord<TEntity>(string sqlQuery, TEntity entity)
        {
            try
            {
                using var connection = CreateConnection();
                return await connection.ExecuteAsync(sqlQuery, entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding record");
                throw;
            }
        }

        public async Task<int> UpdateRecord<TEntity>(string sqlQuery, TEntity entity)
        {
            try
            {
                using var connection = CreateConnection();
                return await connection.ExecuteAsync(sqlQuery, entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating record");
                throw;
            }
        }

        public async Task<int> DeleteRecordById(string sqlQuery, Guid id)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sqlQuery, new { Id = id });
        }

        public async Task<TResult?> GetRecordById<TResult>(string sqlQuery, Guid id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TResult>(sqlQuery, new { Id = id });
        }

        public async Task<TResult?> GetOneByParameter<TResult>(string sqlQuery, string parameterName, object value)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add(parameterName, value);
            return await connection.QueryFirstOrDefaultAsync<TResult>(sqlQuery, parameters);
        }
        
        // --- Specific Methods ---

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
                using var connection = CreateConnection();
        
                // ВАЖЛИВО: Перетворюємо UserId в Guid для перевірки
                var userIdGuid = Guid.Parse(anthropometrydto.UserId.ToString());

                var userCheckSql = "SELECT COUNT(1) FROM Users WHERE Id = @UserId";
                var userExists = await connection.ExecuteScalarAsync<int>(userCheckSql, new { UserId = userIdGuid });
    
                if (userExists == 0)
                {
                    throw new InvalidOperationException($"User with ID {anthropometrydto.UserId} not found");
                }

                int? ageToStore = anthropometrydto.Age;
                if (!ageToStore.HasValue)
                {
                    var getUserAgeSql = "SELECT Age FROM Users WHERE Id = @UserId";
                    ageToStore = await connection.ExecuteScalarAsync<int?>(getUserAgeSql, new { UserId = userIdGuid });
                }
                else
                {
                    var updateUserAgeSql = "UPDATE Users SET Age = @Age WHERE Id = @UserId";
                    await connection.ExecuteAsync(updateUserAgeSql, new { UserId = userIdGuid, Age = ageToStore.Value });
                }

                var parameters = new DynamicParameters();
                parameters.Add("@Id", Guid.NewGuid()); // Генеруємо новий Guid
                parameters.Add("@UserId", userIdGuid); // Використовуємо Guid
                parameters.Add("@MeasuredAt", anthropometrydto.MeasuredAt);
                parameters.Add("@Weight", anthropometrydto.Weight);
                parameters.Add("@Height", anthropometrydto.Height);
                parameters.Add("@Sugar", anthropometrydto.Sugar);
                parameters.Add("@BloodType", anthropometrydto.BloodType);
                parameters.Add("@Age", ageToStore); 

                return await connection.ExecuteAsync(SqlQueries.AddAnthropometry, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAnthrometry");
                throw;
            }
        }

        public async Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await GetRecordsByParameters<AnthropometryDTO>(SqlQueries.GetAnthropometries, parameters);
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
            
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<PasswordResetCode>(SqlQueries.GetValidPasswordResetCode, parameters);
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
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await GetRecordsByParameters<UserNoteDTO>(SqlQueries.GetUserNotes, parameters);
        }

        public async Task<bool> DeleteUserNote(Guid userId, Guid noteId)
        {
            using var connection = CreateConnection();
            var parameters = new { UserId = userId, Id = noteId };
            var rows = await connection.ExecuteAsync(SqlQueries.DeleteUserNote, parameters);
            return rows > 0;
        }

        public async Task<Guid> AddDoctorVisit(DoctorVisit visit)
        {
            visit.Id = Guid.NewGuid();
            await AddRecord(SqlQueries.AddDoctorVisit, visit);
            return visit.Id;
        }

        public async Task<List<DoctorVisit>> GetDoctorVisits(Guid userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await GetRecordsByParameters<DoctorVisit>(SqlQueries.GetDoctorVisits, parameters);
        }

        public async Task<UserFile?> GetUserFileById(Guid fileId, Guid userId)
        {
            using var connection = CreateConnection();
            const string sql = "SELECT * FROM UserFiles WHERE Id = @Id AND UserId = @UserId";
            return await connection.QuerySingleOrDefaultAsync<UserFile>(sql, new { Id = fileId, UserId = userId });
        }

        public async Task<int> DeleteUserFileById(Guid fileId, Guid userId)
        {
            using var connection = CreateConnection();
            const string sql = "DELETE FROM UserFiles WHERE Id = @Id AND UserId = @UserId";
            return await connection.ExecuteAsync(sql, new { Id = fileId, UserId = userId });
        }

        public async Task<List<UserFile>> GetUserFilesByUserId(Guid userId)
        {
             var parameters = new DynamicParameters();
             parameters.Add("@UserId", userId);
             return await GetRecordsByParameters<UserFile>(SqlQueries.GetUserFilesByUserId, parameters);
        }

        // --- MEDICATION METHODS (ВИПРАВЛЕНО) ---

        public async Task<int> AddMedication(Medication medication)
        {
            if (medication.Id == Guid.Empty) 
            {
                medication.Id = Guid.NewGuid();
            }
            
            medication.CreatedAt = DateTime.UtcNow;
            medication.UpdatedAt = DateTime.UtcNow;
            if (medication.StartDate == default) medication.StartDate = DateTime.Today;
            if (medication.EndDate == null) medication.EndDate = CalculateEndDate(medication.Duration);
            medication.WeekDaysJson ??= "[]";
            medication.TimesJson ??= "[]";

            var parameters = new DynamicParameters();
            // Передаємо Guid напряму, бо в моделі вони вже Guid
            parameters.Add("@Id", medication.Id); 
            parameters.Add("@UserId", medication.UserId); 
            
            parameters.Add("@NameOfMedication", medication.NameOfMedication);
            parameters.Add("@Dose", medication.Dose);
            parameters.Add("@TimesJson", medication.TimesJson);
            parameters.Add("@WeekDaysJson", medication.WeekDaysJson);
            parameters.Add("@Type", medication.Type.ToString()); 
            parameters.Add("@Duration", medication.Duration);
            parameters.Add("@StartDate", medication.StartDate);
            parameters.Add("@EndDate", medication.EndDate);
            parameters.Add("@CreatedAt", medication.CreatedAt);
            parameters.Add("@UpdatedAt", medication.UpdatedAt);

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(SqlQueries.AddMedication, parameters);
        }

        // Цей метод був пропущений, тепер додано
        public async Task<List<Medication>> GetMedications(Guid userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await GetRecordsByParameters<Medication>(SqlQueries.GetMedications, parameters);
        }

        public async Task<int> UpdateMedication(Medication medication)
        {
            medication.UpdatedAt = DateTime.UtcNow;
            if (medication.EndDate == null) medication.EndDate = CalculateEndDate(medication.Duration);

            var parameters = new DynamicParameters();
            // Передаємо Guid напряму
            parameters.Add("@Id", medication.Id);       
            parameters.Add("@UserId", medication.UserId); 
            
            parameters.Add("@NameOfMedication", medication.NameOfMedication);
            parameters.Add("@Dose", medication.Dose);
            parameters.Add("@TimesJson", medication.TimesJson);
            parameters.Add("@WeekDaysJson", medication.WeekDaysJson);
            parameters.Add("@Type", medication.Type.ToString());
            parameters.Add("@Duration", medication.Duration);
            parameters.Add("@StartDate", medication.StartDate);
            parameters.Add("@EndDate", medication.EndDate);
            parameters.Add("@UpdatedAt", medication.UpdatedAt);

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(SqlQueries.UpdateMedication, parameters);
        }

        public async Task<bool> DeleteMedication(Guid id)
        {
            return await DeleteRecordById(SqlQueries.DeleteMedication, id) > 0;
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
      
        public async Task<int> AddSleep(SleepDTO sleepDto)
        {
            if (sleepDto.Id == Guid.Empty) sleepDto.Id = Guid.NewGuid();
            return await AddRecord(SqlQueries.AddSleep, sleepDto);
        }

        public async Task<List<SleepDTO>> GetSleepRecords(Guid userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await GetRecordsByParameters<SleepDTO>(SqlQueries.GetSleepByUserId, parameters);
        }
    }
}