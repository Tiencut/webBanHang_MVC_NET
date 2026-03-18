using SV22T1020761.DataLayers.SQLServer;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Common;

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// Service cung c?p d? li?u cho domain Catalog (Categories, Products)
    /// </summary>
    public static class CatalogDataService
    {
        private static CategoryRepository _categoryRepo;

        static CatalogDataService()
        {
            _categoryRepo = new CategoryRepository(Configuration.ConnectionString);
        }

        public static PagedResult<Category> ListCategories(PaginationSearchInput input)
            => _categoryRepo.ListAsync(input).GetAwaiter().GetResult();

        public static PagedResult<Product> ListProducts(PaginationSearchInput input)
        {
            // Example implementation for listing products from a database
            using (var context = new ApplicationDbContext()) // Replace with your actual DbContext
            {
                var query = context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(input.SearchValue))
                {
                    query = query.Where(p => p.ProductName.Contains(input.SearchValue));
                }

                var totalItems = query.Count();
                var items = query
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();

                return new PagedResult<Product>
                {
                    Page = input.Page,
                    PageSize = input.PageSize,
                    RowCount = totalItems,
                    DataItems = items
                };
            }
        }

        public static void AddProduct(Product product)
        {
            // Placeholder implementation for adding a product
        }

        public static Product GetProduct(int id)
        {
            // Placeholder implementation for getting a product by ID
            return new Product();
        }

        public static void UpdateProduct(Product product)
        {
            // Placeholder implementation for updating a product
        }

        public static void DeleteProduct(int id)
        {
            // Placeholder implementation for deleting a product
        }

        public static void AddCategory(Category category)
        {
            // Implementation for adding a category
            throw new NotImplementedException();
        }

        public static Category GetCategory(int categoryId)
        {
            // Implementation for retrieving a category by ID
            throw new NotImplementedException();
        }

        public static void UpdateCategory(Category category)
        {
            // Implementation for updating a category
            throw new NotImplementedException();
        }

        public static void DeleteCategory(int categoryId)
        {
            // Implementation for deleting a category
            throw new NotImplementedException();
        }
    }
}
