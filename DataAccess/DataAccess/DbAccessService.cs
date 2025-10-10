using System.Data;
using Dapper;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataAccess.DataAccess
{
    public interface IDbAccessService
    {
        string? GetConnectionString();
        Task<List<TResult>> GetRecords<TResult>(string procedureName);
        Task<int> AddRecord<TEntity>(string procedureName, TEntity entity);
        Task<int> UpdateRecord<TEntity>(string procedureName, TEntity entity);
        Task<int> DeleteRecordById(string procedureName, Guid id);
        Task<TResult?> GetRecordById<TResult>(string procedureName, Guid id);
        Task<TResult?> GetOneByParameter<TResult>(string procedureName, string parameterName, object value);
        
        Task<int> AddHealthMeasurement(HealthMeasurementDTO measurementDto);
        Task<List<HealthMeasurementDTO>> GetHealthMeasurements(Guid userId);
        Task <int>  AddAnthrometry(AnthropometryDTO anthropometrydto);
       
        Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId);

        Task<int> AddPasswordResetCode(PasswordResetCode code);
        Task <PasswordResetCode?> GetValidResetCode(Guid id, int resetCode);
    }

    public class DbAccessService : IDbAccessService
    {
        private readonly IConfiguration _configuration;

        public DbAccessService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string? GetConnectionString()
        {
            try
            {
                return _configuration.GetConnectionString("SqlServer");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving connection string from configuration : {e.Message}");
            }
        }

        public async Task<List<TResult>> GetRecords<TResult>(string procedureName)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var result = await connection.QueryAsync<TResult>(procedureName, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving records using procedure '{procedureName}': {e.Message}");
            }
        }

       
        public async Task<List<TResult>> GetRecordsAsync<T1, T2, T3, T4, TResult>(
            string procedureName,
            Func<T1, T2, T3, T4, TResult> map,
            string splitOn)
        {
            await using var connection = new SqlConnection(GetConnectionString());
            var result = await connection.QueryAsync<T1, T2, T3, T4, TResult>(
                procedureName,
                map,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<TResult?> GetRecordByIdAsync<T1, T2, T3, T4, TResult>(
            string procedureName,
            Func<T1, T2, T3, T4, TResult> map,
            Guid id,
            string splitOn)
        {
            await using var connection = new SqlConnection(GetConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var result = await connection.QueryAsync<T1, T2, T3, T4, TResult>(
                procedureName,
                map,
                param: parameters,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure);
            
            return result.ToList().FirstOrDefault();
        }

        public async Task<List<TResult>> GetRecordsByParametersAsync<T1, T2, T3, T4, TResult>(
            string procedureName,
            Func<T1, T2, T3, T4, TResult> map,
            DynamicParameters parameters,
            string splitOn)
        {
            await using var connection = new SqlConnection(GetConnectionString());
            var result = await connection.QueryAsync<T1, T2, T3, T4, TResult>(
                procedureName,
                map,
                param: parameters,
                splitOn: splitOn,
                commandType: CommandType.StoredProcedure);
            
            return result.ToList();
        }

        public async Task<int> AddRecord<TEntity>(string procedureName, TEntity entity)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                return await connection.ExecuteAsync(procedureName, entity, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error adding record using procedure '{procedureName}': {e.Message}");
            }
        }

        public async Task<int> UpdateRecord<TEntity>(string procedureName, TEntity entity)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                return await connection.ExecuteAsync(procedureName, entity, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error updating record using procedure '{procedureName}': {e.Message}");
            }
        }

        public async Task<int> DeleteRecordById(string procedureName, Guid id)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error deleting record with ID '{id}' using procedure '{procedureName}': {e.Message}");
            }
        }

        public async Task<TResult?> GetRecordById<TResult>(string procedureName, Guid id)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                return await connection.QuerySingleOrDefaultAsync<TResult>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving record with ID '{id}' using procedure '{procedureName}' : {e.Message}");
            } 
        }

        public async Task<TResult?> GetOneByParameter<TResult>(string procedureName, string parameterName, object value)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add(parameterName, value);
                return await connection.QuerySingleOrDefaultAsync<TResult>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving record with parameter '{parameterName}' and value '{value}' using procedure '{procedureName}': {e.Message}");
            }
        }

        public async Task<List<TResult>> GetAllByParameter<TResult>(string procedureName, string parameterName, object value)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add(parameterName, value);

                var result = await connection.QueryAsync<TResult>(procedureName, parameters, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving records with parameter '{parameterName}' and value '{value}' using procedure '{procedureName}': {e.Message}");
            }
        }

       
        public async Task<List<TResult>> GetRecordsByParameters<TResult>(string procedureName, DynamicParameters parameters)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var result = await connection.QueryAsync<TResult>(procedureName, parameters, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving records using procedure '{procedureName}' with parameters: {e.Message}");
            }
        }
        public async Task<int> AddHealthMeasurement(HealthMeasurementDTO measurementDto)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                return await connection.ExecuteAsync(
                    StoredProceduresNames.AddHealthMeasurement,
                    measurementDto,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error adding health measurement: {e.Message}");
            }
        }

        public async Task<List<HealthMeasurementDTO>> GetHealthMeasurements(Guid userId)
        {
           
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var result = await connection.QueryAsync<HealthMeasurementDTO>(
                    StoredProceduresNames.GetMeasurements,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving measurements for user {userId}: {e.Message}");
            }
        }

        public async Task<int> AddAnthrometry(AnthropometryDTO anthropometrydto)
        {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", anthropometrydto.UserId);
                parameters.Add("@MeasuredAt", anthropometrydto.MeasuredAt);
                parameters.Add("@Weight", anthropometrydto.Weight);
                parameters.Add("@Height", anthropometrydto.Height);

                var result = await connection.QuerySingleAsync<int>(
                    "sp_AddAnthropometry",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result;
        }

        public async Task<List<AnthropometryDTO>> GetAnthropometries(Guid userId)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var result = await connection.QueryAsync<AnthropometryDTO>(
                    StoredProceduresNames.GetAnthropometries,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error retrieving measurements for user {userId}: {e.Message}");
            }
        }

        public async Task<int> AddPasswordResetCode(PasswordResetCode code)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", code.UserId);
                parameters.Add("@ResetCode", code.ResetCode);
                parameters.Add("@ExpiresAt", code.ExpiresAt);
                parameters.Add("@IsUsed", code.IsUsed);

                return await connection.ExecuteAsync(
                    "sp_AddPasswordResetCode",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error adding password reset code: {e.Message}");
            }
        }

        public async Task<PasswordResetCode?> GetValidResetCode(Guid id, int resetCode)
        {
            try
            {
                await using var connection = new SqlConnection(GetConnectionString());
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", id); //TODO: змінити значення у бд з userid на email
                parameters.Add("@ResetCode", resetCode);
                return await connection.QuerySingleOrDefaultAsync<PasswordResetCode>(
                    "sp_GetValidPasswordResetCode",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Error getting password reset code", e);
            }
           
        }
    }
    
}
