using SV22T1020761.DataLayers.SQLServer;
using SV22T1020761.DataLayers.SQLServer.Catalog;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Common;
using System; // for Exception
using System.Collections.Generic;

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// Service cung c?p d? li?u cho domain Catalog (Categories, Products)
    /// </summary>
    public static class CatalogDataService
    {
        private static CategoryRepository _categoryRepo;
        private static ProductRepository _productRepo;
        private static SupplierRepository _supplierRepo;

        static CatalogDataService()
        {
            _categoryRepo = new CategoryRepository(Configuration.ConnectionString);
            _productRepo = new ProductRepository(Configuration.ConnectionString);
            _supplierRepo = new SupplierRepository(Configuration.ConnectionString);
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

        public static List<ProductPhoto> ListProductPhotos(int productId)
        {
            return _productRepo.ListPhotosAsync(productId).GetAwaiter().GetResult();
        }

        public static List<ProductAttribute> ListAttributes(int productId)
        {
            return _productRepo.ListAttributesAsync(productId).GetAwaiter().GetResult();
        }

        public static string? GetCategoryName(int? categoryId)
        {
            if (categoryId == null || categoryId == 0) return null;
            var c = _categoryRepo.GetAsync(categoryId.Value).GetAwaiter().GetResult();
            return c?.CategoryName;
        }

        public static string? GetSupplierName(int? supplierId)
        {
            if (supplierId == null || supplierId == 0) return null;
            var s = _supplierRepo.GetAsync(supplierId.Value).GetAwaiter().GetResult();
            return s?.SupplierName;
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
