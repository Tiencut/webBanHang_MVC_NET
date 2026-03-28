using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Partner;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer.Partner
{
    /// <summary>
    /// Repository to manage customers.
    /// </summary>
    public class CustomerRepository : SV22T1020761.DataLayers.Interfaces.ICustomerRepository
    {
        private readonly string _connectionString;
        public CustomerRepository(string connectionString) => _connectionString = connectionString;

        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            var result = new PagedResult<Customer> { Page = input.Page, PageSize = input.PageSize };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string where = string.Empty;
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                where = "WHERE CustomerName LIKE @q OR ContactName LIKE @q OR Phone LIKE @q";
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            var countSql = $"SELECT COUNT(*) FROM Customers {where}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked FROM Customers {where} ORDER BY CustomerID";
            }
            else
            {
                dataSql = $@"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked
FROM Customers {where}
ORDER BY CustomerID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<Customer>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        public async Task<Customer?> GetAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked FROM Customers WHERE CustomerID = @id";
            return await conn.QuerySingleOrDefaultAsync<Customer>(sql, new { id });
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked FROM Customers WHERE Email = @email";
            return await conn.QuerySingleOrDefaultAsync<Customer>(sql, new { email });
        }

        public async Task<int> AddAsync(Customer data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await conn.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Customer data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Customers SET CustomerName=@CustomerName, ContactName=@ContactName, Province=@Province, Address=@Address, Phone=@Phone, Email=@Email, IsLocked=@IsLocked WHERE CustomerID=@CustomerID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Customers WHERE CustomerID = @id";
            var affected = await conn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<bool> IsUsed(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT COUNT(1) FROM Orders WHERE CustomerID = @id";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { id });
            return cnt > 0;
        }

        // implement interface method
        public Task<bool> IsUsedAsync(int id) => IsUsed(id);

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = id == 0 ? "SELECT COUNT(1) FROM Customers WHERE Email = @email" : "SELECT COUNT(1) FROM Customers WHERE Email = @email AND CustomerID <> @id";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { email, id });
            return cnt == 0;
        }
    }
}
