using SV22T1020761.DataLayers.SQLServer;
using SV22T1020761.DataLayers.SQLServer.Catalog;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Common;
using System; // for Exception

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// Service cung c?p d? li?u cho domain Catalog (Categories, Products)
    /// </summary>
    public static class CatalogDataService
    {
        private static CategoryRepository _categoryRepo;
        private static ProductRepository _productRepo;

        static CatalogDataService()
        {
            _categoryRepo = new CategoryRepository(Configuration.ConnectionString);
            _productRepo = new ProductRepository(Configuration.ConnectionString);
        }

        public static PagedResult<Category> ListCategories(PaginationSearchInput input)
            => _categoryRepo.ListAsync(input).GetAwaiter().GetResult();

        public static PagedResult<Product> ListProducts(PaginationSearchInput input)
        {
            try
            {
                // Map PaginationSearchInput to ProductSearchInput
                var pInput = new ProductSearchInput
                {
                    Page = input?.Page ?? 1,
                    PageSize = input?.PageSize ?? 10,
                    SearchValue = input?.SearchValue ?? string.Empty
                };
                return _productRepo.ListAsync(pInput).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // Avoid throwing raw exceptions to UI. Log minimal info and return empty result.
                // If you have a logger, replace the Console with logger.
                System.Diagnostics.Trace.TraceError("CatalogDataService.ListProducts error: {0}", ex.Message);
                return new PagedResult<Product> { Page = input?.Page ?? 1, PageSize = input?.PageSize ?? 10, RowCount = 0, DataItems = new System.Collections.Generic.List<Product>() };
            }
        }

        public static void AddProduct(Product product)
        {
            if (product == null) return;
            _productRepo.AddAsync(product).GetAwaiter().GetResult();
        }

        public static Product GetProduct(int id)
        {
            return _productRepo.GetAsync(id).GetAwaiter().GetResult() ?? new Product();
        }

        public static void UpdateProduct(Product product)
        {
            if (product == null) return;
            _productRepo.UpdateAsync(product).GetAwaiter().GetResult();
        }

        public static void DeleteProduct(int id)
        {
            _productRepo.DeleteAsync(id).GetAwaiter().GetResult();
        }

        public static void AddCategory(Category category)
        {
            _categoryRepo.AddAsync(category).GetAwaiter().GetResult();
        }

        public static Category GetCategory(int categoryId)
        {
            return _categoryRepo.GetAsync(categoryId).GetAwaiter().GetResult() ?? new Category();
        }

        public static void UpdateCategory(Category category)
        {
            _categoryRepo.UpdateAsync(category).GetAwaiter().GetResult();
        }

        public static void DeleteCategory(int categoryId)
        {
            _categoryRepo.DeleteAsync(categoryId).GetAwaiter().GetResult();
        }
    }
}
