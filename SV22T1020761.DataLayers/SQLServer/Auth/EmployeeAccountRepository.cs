using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020761.DataLayers.SQLServer.Auth
{
    /// <summary>
    /// Repository for employee accounts (authorize/change password).
    /// </summary>
    public class EmployeeAccountRepository : SV22T1020761.DataLayers.Interfaces.IUserAccountRepository
    {
        private readonly string _connectionString;
        public EmployeeAccountRepository(string connectionString) => _connectionString = connectionString;

        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            // Assuming Employees table stores Email as username and Password column
            var sql = @"SELECT EmployeeID AS UserId, Email AS UserName, FullName AS DisplayName, Email, Photo, RoleNames
FROM Employees
WHERE Email = @userName AND Password = @password AND IsWorking = 1";
            var acc = await conn.QuerySingleOrDefaultAsync<UserAccount>(sql, new { userName, password });
            return acc;
        }

        public async Task<bool> ChangePassword(string userName, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE Employees SET Password = @password WHERE Email = @userName";
            var affected = await conn.ExecuteAsync(sql, new { userName, password });
            return affected > 0;
        }
    }
}
