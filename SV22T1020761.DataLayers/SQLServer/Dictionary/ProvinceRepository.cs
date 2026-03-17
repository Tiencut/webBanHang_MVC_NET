using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer.Dictionary
{
    /// <summary>
    /// Repository for provinces data dictionary.
    /// </summary>
    public class ProvinceRepository : SV22T1020761.DataLayers.Interfaces.IGenericRepository<string>
    {
        private readonly string _connectionString;
        public ProvinceRepository(string connectionString) => _connectionString = connectionString;

        public async Task<PagedResult<string>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            var result = new PagedResult<string> { Page = input.Page, PageSize = input.PageSize };
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var where = string.Empty;
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                where = "WHERE ProvinceName LIKE @q";
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            var countSql = $"SELECT COUNT(*) FROM Provinces {where}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT ProvinceName FROM Provinces {where} ORDER BY ProvinceName";
            }
            else
            {
                dataSql = $@"SELECT ProvinceName FROM Provinces {where} ORDER BY ProvinceName
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<string>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        public async Task<string?> GetAsync(int id)
        {
            // Provinces primary key is string, so treat id as index (not used). Provide a simple implementation.
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT TOP 1 ProvinceName FROM Provinces ORDER BY ProvinceName";
            return await conn.QuerySingleOrDefaultAsync<string>(sql);
        }

        public async Task<int> AddAsync(string data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "INSERT INTO Provinces (ProvinceName) VALUES (@data)";
            var affected = await conn.ExecuteAsync(sql, new { data });
            return affected;
        }

        public async Task<bool> UpdateAsync(string data)
        {
            // No meaningful update for ProvinceName primary key
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Not implemented because primary key is string
            return false;
        }

        public async Task<bool> IsUsed(int id)
        {
            // Not implemented
            return false;
        }

        // Interface method
        public Task<bool> IsUsedAsync(int id) => IsUsed(id);
    }
}
