using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Partner;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer
{
    /// <summary>
    /// Repository implementation for <see cref="Supplier"/> using SQL Server and Dapper.
    /// Implements <see cref="SV22T1020761.DataLayers.Interfaces.IGenericRepository{T}"/> semantics for Supplier.
    /// </summary>
    public class SupplierRepository : SV22T1020761.DataLayers.Interfaces.IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialize a new instance of <see cref="SupplierRepository"/>.
        /// </summary>
        /// <param name="connectionString">The SQL Server connection string to use.</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Query suppliers with paging and optional search. Returns a <see cref="PagedResult{Supplier}"/>.
        /// </summary>
        /// <param name="input">Pagination and search input.</param>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            input = input ?? new PaginationSearchInput();

            var result = new PagedResult<Supplier>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string where = string.Empty;
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                where = "WHERE SupplierName LIKE @q OR ContactName LIKE @q";
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            // total count
            var countSql = $"SELECT COUNT(*) FROM Suppliers {where}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            // data
            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email FROM Suppliers {where} ORDER BY SupplierID";
            }
            else
            {
                dataSql = $@"SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email
FROM Suppliers {where}
ORDER BY SupplierID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<Supplier>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        /// <summary>
        /// Get a supplier by id.
        /// </summary>
        /// <param name="id">Supplier id.</param>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email FROM Suppliers WHERE SupplierID = @id";
            return await conn.QuerySingleOrDefaultAsync<Supplier>(sql, new { id });
        }

        /// <summary>
        /// Add a new supplier and return generated identity id.
        /// </summary>
        /// <param name="data">Supplier data to add.</param>
        public async Task<int> AddAsync(Supplier data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
INSERT INTO Suppliers (SupplierName, ContactName, Province, Address, Phone, Email)
VALUES (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            var id = await conn.ExecuteScalarAsync<int>(sql, data);
            return id;
        }

        /// <summary>
        /// Update an existing supplier.
        /// </summary>
        /// <param name="data">Supplier data to update (SupplierID must be set).</param>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Suppliers SET SupplierName = @SupplierName, ContactName = @ContactName, Province = @Province,
Address = @Address, Phone = @Phone, Email = @Email WHERE SupplierID = @SupplierID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        /// <summary>
        /// Delete supplier by id.
        /// </summary>
        /// <param name="id">Supplier id to delete.</param>
        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Suppliers WHERE SupplierID = @id";
            var affected = await conn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        /// <summary>
        /// Check whether supplier is used by related data (e.g., Products).
        /// </summary>
        /// <param name="id">Supplier id to check.</param>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT COUNT(1) FROM Orders WHERE SupplierID = @SupplierID";
            return await connection.ExecuteScalarAsync<bool>(query, new { SupplierID = id });
        }
    }
}
