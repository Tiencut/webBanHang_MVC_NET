using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SV22T1020761.Shop.Controllers
{
    public class ProductsController : Controller
    {
        // GET: /Products
        public IActionResult Index(int categoryId = 0, string searchValue = "", decimal minPrice = 0, decimal maxPrice = 0, int page = 1, int pageSize = 10)
        {
            try
            {
                var input = new PaginationSearchInput { SearchValue = searchValue, Page = page, PageSize = pageSize };
                var model = SV22T1020761.BusinessLayers.CatalogDataService.ListProducts(input);

                // load categories for filter
                var cats = SV22T1020761.BusinessLayers.CatalogDataService.ListCategories(new PaginationSearchInput { Page = 1, PageSize = 1000 });
                ViewBag.Categories = cats.DataItems;

                // pass filter values
                ViewBag.FilterCategoryId = categoryId;
                ViewBag.FilterMinPrice = minPrice;
                ViewBag.FilterMaxPrice = maxPrice;
                ViewBag.FilterSearchValue = searchValue;

                return View(model);
            }
            catch (Exception ex)
            {
                // Log and show friendly message
                System.Diagnostics.Trace.TraceError("ProductsController.Index error: {0}", ex.Message);
                TempData["Error"] = "Không th? k?t n?i t?i cõ s? d? li?u. Vui l?ng ki?m tra c?u h?nh và th? l?i.";
                var empty = new PagedResult<Product> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<Product>() };
                ViewBag.Categories = new System.Collections.Generic.List<Category>();
                return View(empty);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(PaginationSearchInput input)
        {
            var result = SV22T1020761.BusinessLayers.CatalogDataService.ListProducts(input);
            return PartialView("_ProductGrid", result);
        }

        // GET: /Products/Details/5
        public IActionResult Details(int id)
        {
            var product = SV22T1020761.BusinessLayers.CatalogDataService.GetProduct(id);
            if (product == null) return NotFound();

            var photos = SV22T1020761.BusinessLayers.CatalogDataService.ListProductPhotos(id);
            var attrs = SV22T1020761.BusinessLayers.CatalogDataService.ListAttributes(id);
            var catName = SV22T1020761.BusinessLayers.CatalogDataService.GetCategoryName(product.CategoryID);
            var supName = SV22T1020761.BusinessLayers.CatalogDataService.GetSupplierName(product.SupplierID);

            var vm = new ProductDetailsViewModel
            {
                Product = product,
                Photos = photos ?? new List<ProductPhoto>(),
                Attributes = attrs ?? new List<ProductAttribute>(),
                CategoryName = catName,
                SupplierName = supName
            };

            return View(vm);
        }

        // GET: /Products/QuickView/5
        public IActionResult QuickView(int id)
        {
            var product = SV22T1020761.BusinessLayers.CatalogDataService.GetProduct(id);
            if (product == null) return NotFound();
            return PartialView("_ProductQuickView", product);
        }
    }
}
