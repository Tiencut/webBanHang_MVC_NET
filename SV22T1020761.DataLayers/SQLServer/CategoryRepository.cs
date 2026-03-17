using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer
{
    /// <summary>
    /// Repository implementation for <see cref="Category"/> using SQL Server and Dapper.
    /// Implements generic repository semantics for Category.
    /// </summary>
    public class CategoryRepository : SV22T1020761.DataLayers.Interfaces.IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialize a new instance of <see cref="CategoryRepository"/>.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string.</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// List categories with paging and optional search.
        /// </summary>
        /// <param name="input">Pagination and search input.</param>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            var result = new PagedResult<Category> { Page = input.Page, PageSize = input.PageSize };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string where = string.Empty;
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                where = "WHERE CategoryName LIKE @q OR Description LIKE @q";
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            var countSql = $"SELECT COUNT(*) FROM Categories {where}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT CategoryID, CategoryName, Description FROM Categories {where} ORDER BY CategoryID";
            }
            else
            {
                dataSql = $@"SELECT CategoryID, CategoryName, Description
FROM Categories {where}
ORDER BY CategoryID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<Category>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        /// <summary>
        /// Get a category by id.
        /// </summary>
        /// <param name="id">Category id.</param>
        public async Task<Category?> GetAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT CategoryID, CategoryName, Description FROM Categories WHERE CategoryID = @id";
            return await conn.QuerySingleOrDefaultAsync<Category>(sql, new { id });
        }

        /// <summary>
        /// Add a new category and return generated id.
        /// </summary>
        /// <param name="data">Category data.</param>
        public async Task<int> AddAsync(Category data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Categories (CategoryName, Description)
VALUES (@CategoryName, @Description);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            var id = await conn.ExecuteScalarAsync<int>(sql, data);
            return id;
        }

        /// <summary>
        /// Update an existing category.
        /// </summary>
        /// <param name="data">Category data to update.</param>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE Categories SET CategoryName = @CategoryName, Description = @Description WHERE CategoryID = @CategoryID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        /// <summary>
        /// Delete category by id.
        /// </summary>
        /// <param name="id">Category id.</param>
        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Categories WHERE CategoryID = @id";
            var affected = await conn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        /// <summary>
        /// Check whether category is used by Products.
        /// </summary>
        /// <param name="id">Category id to check.</param>
        public async Task<bool> IsUsed(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT COUNT(1) FROM Products WHERE CategoryID = @id";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { id });
            return cnt > 0;
        }

        // Implementation required by IGenericRepository<T>
        public Task<bool> IsUsedAsync(int id) => IsUsed(id);
    }
}
