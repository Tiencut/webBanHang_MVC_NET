using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.HR;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer.HR
{
    /// <summary>
    /// Repository to manage employees.
    /// </summary>
    public class EmployeeRepository : SV22T1020761.DataLayers.Interfaces.IEmployeeRepository
    {
        private readonly string _connectionString;
        public EmployeeRepository(string connectionString) => _connectionString = connectionString;

        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            var result = new PagedResult<Employee> { Page = input.Page, PageSize = input.PageSize };
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string where = string.Empty;
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                where = "WHERE FullName LIKE @q OR Phone LIKE @q";
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            var countSql = $"SELECT COUNT(*) FROM Employees {where}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Photo, IsWorking, RoleNames FROM Employees {where} ORDER BY EmployeeID";
            }
            else
            {
                dataSql = $@"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Photo, IsWorking, RoleNames
FROM Employees {where}
ORDER BY EmployeeID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<Employee>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        public async Task<Employee?> GetAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Photo, IsWorking, RoleNames FROM Employees WHERE EmployeeID = @id";
            return await conn.QuerySingleOrDefaultAsync<Employee>(sql, new { id });
        }

        public async Task<int> AddAsync(Employee data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Employees (FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames)
VALUES (@FullName, @BirthDate, @Address, @Phone, @Email, @Password, @Photo, @IsWorking, @RoleNames);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await conn.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Employee data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Employees SET FullName=@FullName, BirthDate=@BirthDate, Address=@Address, Phone=@Phone, Email=@Email, Photo=@Photo, IsWorking=@IsWorking, RoleNames=@RoleNames WHERE EmployeeID=@EmployeeID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Employees WHERE EmployeeID = @id";
            var affected = await conn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<bool> IsUsed(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            // check orders assigned
            var sql = "SELECT COUNT(1) FROM Orders WHERE EmployeeID = @id";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { id });
            return cnt > 0;
        }

        // implement interface method
        public Task<bool> IsUsedAsync(int id) => IsUsed(id);

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = id == 0 ? "SELECT COUNT(1) FROM Employees WHERE Email = @email" : "SELECT COUNT(1) FROM Employees WHERE Email = @email AND EmployeeID <> @id";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { email, id });
            return cnt == 0;
        }
    }
}
