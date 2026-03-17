using SV22T1020761.DataLayers.SQLServer.Dictionary;
using SV22T1020761.Models.Common;

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// Service cung c?p d? li?u cho các b?ng d? li?u chu?n (dictionary) nhý Provinces
    /// </summary>
    public static class DictionaryDataService
    {
        private static ProvinceRepository _provinceRepo;

        static DictionaryDataService()
        {
            _provinceRepo = new ProvinceRepository(Configuration.ConnectionString);
        }

        public static PagedResult<string> ListProvinces(PaginationSearchInput input)
        {
            return _provinceRepo.ListAsync(input).GetAwaiter().GetResult();
        }
    }
}
