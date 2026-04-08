using SV22T1020761.DataLayers.SQLServer;
using SV22T1020761.DataLayers.SQLServer.Catalog;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Common;
using System; // for Exception
using System.Collections.Generic;

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// Service cung c?p dữ liệu cho domain Catalog (Categories, Products)
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
                // Map to ProductSearchInput to include CategoryID and SupplierID
                ProductSearchInput pInput;
                if (input is ProductSearchInput productInput)
                {
                    pInput = productInput;
                }
                else
                {
                    pInput = new ProductSearchInput
                    {
                        Page = input?.Page ?? 1,
                        PageSize = input?.PageSize ?? 10,
                        SearchValue = input?.SearchValue ?? string.Empty,
                        CategoryID = input?.CategoryID ?? 0,
                        SupplierID = input?.SupplierID ?? 0
                    };
                }
                
                System.Diagnostics.Trace.TraceInformation($"CatalogDataService.ListProducts - Page: {pInput.Page}, PageSize: {pInput.PageSize}, SearchValue: {pInput.SearchValue}, CategoryID: {pInput.CategoryID}, SupplierID: {pInput.SupplierID}");
                
                var result = _productRepo.ListAsync(pInput).GetAwaiter().GetResult();
                System.Diagnostics.Trace.TraceInformation($"CatalogDataService.ListProducts - Result: RowCount={result.RowCount}, DataItems={result.DataItems?.Count ?? 0}");
                return result;
            }
            catch (Exception ex)
            {
                string errorMsg = $"CatalogDataService.ListProducts error - Message: {ex.Message} | StackTrace: {ex.StackTrace}";
                System.Diagnostics.Trace.TraceError(errorMsg);
                Console.WriteLine($"❌ {errorMsg}");
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

        // =====================================================
        // ProductPhoto methods
        // =====================================================
        public static ProductPhoto GetProductPhoto(long photoId)
        {
            return _productRepo.GetPhotoAsync(photoId).GetAwaiter().GetResult() ?? new ProductPhoto();
        }

        public static void AddProductPhoto(ProductPhoto photo)
        {
            if (photo == null) return;
            _productRepo.AddPhotoAsync(photo).GetAwaiter().GetResult();
        }

        public static void UpdateProductPhoto(ProductPhoto photo)
        {
            if (photo == null) return;
            _productRepo.UpdatePhotoAsync(photo).GetAwaiter().GetResult();
        }

        public static void DeleteProductPhoto(long photoId)
        {
            _productRepo.DeletePhotoAsync(photoId).GetAwaiter().GetResult();
        }

        // =====================================================
        // ProductAttribute methods
        // =====================================================
        public static ProductAttribute GetProductAttribute(int attributeId)
        {
            return _productRepo.GetAttributeAsync(attributeId).GetAwaiter().GetResult() ?? new ProductAttribute();
        }

        public static void AddProductAttribute(ProductAttribute attribute)
        {
            if (attribute == null) return;
            _productRepo.AddAttributeAsync(attribute).GetAwaiter().GetResult();
        }

        public static void UpdateProductAttribute(ProductAttribute attribute)
        {
            if (attribute == null) return;
            _productRepo.UpdateAttributeAsync(attribute).GetAwaiter().GetResult();
        }

        public static void DeleteProductAttribute(int attributeId)
        {
            _productRepo.DeleteAttributeAsync(attributeId).GetAwaiter().GetResult();
        }
    }
}
