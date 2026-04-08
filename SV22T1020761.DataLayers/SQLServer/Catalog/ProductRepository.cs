using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer.Catalog
{
    /// <summary>
    /// Repository for Product and related entities (attributes, photos).
    /// </summary>
    public class ProductRepository : SV22T1020761.DataLayers.Interfaces.IProductRepository
    {
        private readonly string _connectionString;
        public ProductRepository(string connectionString) => _connectionString = connectionString;

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            input ??= new ProductSearchInput();
            var result = new PagedResult<Product> { Page = input.Page, PageSize = input.PageSize };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            if (input.CategoryID > 0) { whereClauses.Add("CategoryID = @CategoryID"); parameters.Add("CategoryID", input.CategoryID); }
            if (input.SupplierID > 0) { whereClauses.Add("SupplierID = @SupplierID"); parameters.Add("SupplierID", input.SupplierID); }
            if (input.MinPrice > 0) { whereClauses.Add("Price >= @MinPrice"); parameters.Add("MinPrice", input.MinPrice); }
            if (input.MaxPrice > 0) { whereClauses.Add("Price <= @MaxPrice"); parameters.Add("MaxPrice", input.MaxPrice); }
            if (!string.IsNullOrWhiteSpace(input.SearchValue)) 
            { 
                var searchTerm = "%" + input.SearchValue + "%";
                Console.WriteLine($"🔍 DEBUG: SearchValue='{input.SearchValue}', searchTerm='{searchTerm}'");
                whereClauses.Add("UPPER(ProductName) LIKE UPPER(@q)"); 
                parameters.Add("q", searchTerm); 
            }

            string where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;
  
            var countSql = $"SELECT COUNT(*) FROM Products {where}";
            System.Diagnostics.Trace.TraceInformation($"ProductRepository.ListAsync - CountSql: {countSql}");
            Console.WriteLine($"📊 ProductRepository.ListAsync - CountSql: {countSql}");
            
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            System.Diagnostics.Trace.TraceInformation($"ProductRepository.ListAsync - RowCount: {result.RowCount}");
            Console.WriteLine($"📊 ProductRepository.ListAsync - RowCount: {result.RowCount}");

            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling FROM Products {where} ORDER BY ProductID";
            }
            else
            {
                dataSql = $@"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling
FROM Products {where}
ORDER BY ProductID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            System.Diagnostics.Trace.TraceInformation($"ProductRepository.ListAsync - DataSql: {dataSql}");
            Console.WriteLine($"📊 ProductRepository.ListAsync - DataSql: {dataSql}");
            
            var items = await conn.QueryAsync<Product>(dataSql, parameters);
            result.DataItems = items.AsList();
            System.Diagnostics.Trace.TraceInformation($"ProductRepository.ListAsync - Items loaded: {result.DataItems.Count}");
            Console.WriteLine($"📊 ProductRepository.ListAsync - Items loaded: {result.DataItems.Count}");
            
            return result;
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling FROM Products WHERE ProductID = @productID";
            return await conn.QuerySingleOrDefaultAsync<Product>(sql, new { productID });
        }

        public async Task<int> AddAsync(Product data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await conn.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Products SET ProductName=@ProductName, ProductDescription=@ProductDescription, SupplierID=@SupplierID, CategoryID=@CategoryID, Unit=@Unit, Price=@Price, Photo=@Photo, IsSelling=@IsSelling WHERE ProductID=@ProductID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Products WHERE ProductID = @productID";
            var affected = await conn.ExecuteAsync(sql, new { productID });
            return affected > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT COUNT(1) FROM OrderDetails WHERE ProductID = @productID";
            var cnt = await conn.ExecuteScalarAsync<int>(sql, new { productID });
            return cnt > 0;
        }

        // Attributes
        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder FROM ProductAttributes WHERE ProductID = @productID ORDER BY DisplayOrder";
            var items = await conn.QueryAsync<ProductAttribute>(sql, new { productID });
            return items.AsList();
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder FROM ProductAttributes WHERE AttributeID = @attributeID";
            return await conn.QuerySingleOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await conn.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE ProductAttributes SET AttributeName=@AttributeName, AttributeValue=@AttributeValue, DisplayOrder=@DisplayOrder WHERE AttributeID=@AttributeID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM ProductAttributes WHERE AttributeID = @attributeID";
            var affected = await conn.ExecuteAsync(sql, new { attributeID });
            return affected > 0;
        }

        // Photos
        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden FROM ProductPhotos WHERE ProductID = @productID ORDER BY DisplayOrder";
            var items = await conn.QueryAsync<ProductPhoto>(sql, new { productID });
            return items.AsList();
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden FROM ProductPhotos WHERE PhotoID = @photoID";
            return await conn.QuerySingleOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await conn.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE ProductPhotos SET Photo=@Photo, Description=@Description, DisplayOrder=@DisplayOrder, IsHidden=@IsHidden WHERE PhotoID=@PhotoID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM ProductPhotos WHERE PhotoID = @photoID";
            var affected = await conn.ExecuteAsync(sql, new { photoID });
            return affected > 0;
        }
    }
}
