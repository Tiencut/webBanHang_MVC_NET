using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020761.DataLayers.SQLServer.Auth
{
    /// <summary>
    /// Repository for customer accounts (authorize/change password).
    /// </summary>
    public class CustomerAccountRepository : SV22T1020761.DataLayers.Interfaces.IUserAccountRepository
    {
        private readonly string _connectionString;
        public CustomerAccountRepository(string connectionString) => _connectionString = connectionString;

        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT CustomerID AS UserId, Email AS UserName, CustomerName AS DisplayName, Email, '' AS Photo, '' AS RoleNames
FROM Customers
WHERE Email = @userName AND Password = @password AND IsLocked = 0";
            var acc = await conn.QuerySingleOrDefaultAsync<UserAccount>(sql, new { userName, password });
            return acc;
        }

        public async Task<bool> ChangePassword(string userName, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE Customers SET Password = @password WHERE Email = @userName";
            var affected = await conn.ExecuteAsync(sql, new { userName, password });
            return affected > 0;
        }
    }
}
