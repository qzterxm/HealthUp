using System.Data;
using Dapper;
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
        Task<List<TResult>> GetAllByParameter<TResult>(string procedureName, string parameterName, object value);
        Task<List<TResult>> GetRecordsByParameters<TResult>(string procedureName, DynamicParameters parameters);
        Task<List<TResult>> GetRecordsAsync<T1, T2, T3, T4, TResult>(
            string procedureName,
            Func<T1, T2, T3, T4, TResult> map,
            string splitOn);
        Task<TResult?> GetRecordByIdAsync<T1, T2, T3, T4, TResult>(
            string procedureName,
            Func<T1, T2, T3, T4, TResult> map,
            Guid id,
            string splitOn);
        Task<List<TResult>> GetRecordsByParametersAsync<T1, T2, T3, T4, TResult>(
            string procedureName,
            Func<T1, T2, T3, T4, TResult> map,
            DynamicParameters parameters,
            string splitOn);
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

    }
}
