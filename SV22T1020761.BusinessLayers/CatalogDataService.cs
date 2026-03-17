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
    }
}
