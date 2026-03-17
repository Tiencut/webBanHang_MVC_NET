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
    /// Repository implementation for <see cref="Shipper"/> using SQL Server and Dapper.
    /// Implements generic repository semantics for Shipper.
    /// </summary>
    public class ShipperRepository : SV22T1020761.DataLayers.Interfaces.IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialize a new instance of <see cref="ShipperRepository"/>.
        /// </summary>
        /// <param name="connectionString">The SQL Server connection string to use.</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Query shippers with paging and optional search. Returns a <see cref="PagedResult{Shipper}"/>.
        /// </summary>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            input = input ?? new PaginationSearchInput();

            var result = new PagedResult<Shipper>
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
                where = "WHERE ShipperName LIKE @q OR Phone LIKE @q";
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            // total count
            var countSql = $"SELECT COUNT(*) FROM Shippers {where}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            // data
            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT ShipperID, ShipperName, Phone FROM Shippers {where} ORDER BY ShipperID";
            }
            else
            {
                dataSql = $@"SELECT ShipperID, ShipperName, Phone
FROM Shippers {where}
ORDER BY ShipperID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<Shipper>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        /// <summary>
        /// Get a shipper by id.
        /// </summary>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT ShipperID, ShipperName, Phone FROM Shippers WHERE ShipperID = @id";
            return await conn.QuerySingleOrDefaultAsync<Shipper>(sql, new { id });
        }

        /// <summary>
        /// Add a new shipper and return generated identity id.
        /// </summary>
        public async Task<int> AddAsync(Shipper data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Shippers (ShipperName, Phone)
VALUES (@ShipperName, @Phone);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            var id = await conn.ExecuteScalarAsync<int>(sql, data);
            return id;
        }

        /// <summary>
        /// Update an existing shipper.
        /// </summary>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE Shippers SET ShipperName = @ShipperName, Phone = @Phone WHERE ShipperID = @ShipperID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        /// <summary>
        /// Delete shipper by id.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Shippers WHERE ShipperID = @id";
            var affected = await conn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        /// <summary>
        /// Check whether shipper is used by Orders.
        /// </summary>
        public async Task<bool> IsUsed(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT COUNT(1) FROM Orders WHERE ShipperID = @id";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { id });
            return cnt > 0;
        }

        public Task<bool> IsUsedAsync(int id) => IsUsed(id);
    }
}
