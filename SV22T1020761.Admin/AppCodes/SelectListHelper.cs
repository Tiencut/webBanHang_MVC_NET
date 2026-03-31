using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Sales;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.Admin
{
    /// <summary>
    /// Lớp cung cấp các hàm tiện ích dùng cho SelectList (DropDownList)
    /// </summary>
    public static class SelectListHelper
    {
        /// <summary>
        /// Tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static Task<List<SelectListItem>> Provinces()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "", Text = "-- Tỉnh/Thành phố --"}
            };
            var input = new PaginationSearchInput() { Page = 1, PageSize = 0, SearchValue = "" };
            var result = DictionaryDataService.ListProvinces(input);
            if (result?.DataItems != null)
            {
                foreach (var item in result.DataItems)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = item,
                        Text = item
                    });
                }
            }
            return Task.FromResult(list);
        }

        /// <summary>
        /// Loại hàng
        /// </summary>
        /// <returns></returns>
        public static Task<List<SelectListItem>> Categories()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "-- Loại hàng --"}
            };
            var input = new PaginationSearchInput() { Page = 1, PageSize = 0, SearchValue = "" };
            var result = CatalogDataService.ListCategories(input);
            if (result?.DataItems != null)
            {
                foreach (var item in result.DataItems)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = item.CategoryID.ToString(),
                        Text = item.CategoryName
                    });
                }
            }
            return Task.FromResult(list);
        }

        /// <summary>
        /// Nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public static async Task<List<SelectListItem>> Suppliers()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "-- Nhà cung cấp --"}
            };
            var input = new PaginationSearchInput() { Page = 1, PageSize = 0, SearchValue = "" };
            var result = await PartnerDataService.ListSuppliersAsync(input);
            if (result?.DataItems != null)
            {
                foreach (var item in result.DataItems)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = item.SupplierID.ToString(),
                        Text = item.SupplierName
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Các trạng thái của đơn hàng
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> OrderStatus()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() { Value = "", Text = "-- Trạng thái ---" },
                new SelectListItem() { Value = OrderStatusEnum.New.ToString(), Text = OrderStatusEnum.New.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Accepted.ToString(), Text = OrderStatusEnum.Accepted.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Shipping.ToString(), Text = OrderStatusEnum.Shipping.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Completed.ToString(), Text = OrderStatusEnum.Completed.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Rejected.ToString(), Text = OrderStatusEnum.Rejected.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Cancelled.ToString(), Text = OrderStatusEnum.Cancelled.GetDescription() },
            };
        }
    }
}