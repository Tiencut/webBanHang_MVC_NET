using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SV22T1020761.Shop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ICompositeViewEngine _viewEngine;

        public ProductsController(ICompositeViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
        }

        private string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (viewResult.Success)
                {
                    var viewContext = new ViewContext(
                        ControllerContext,
                        viewResult.View,
                        ViewData,
                        TempData,
                        sw,
                        new HtmlHelperOptions()
                    );
                    viewResult.View.RenderAsync(viewContext).Wait();
                    return sw.ToString();
                }
                else
                {
                    throw new Exception($"View {viewName} not found");
                }
            }
        }

        // GET: /Products
        public IActionResult Index(int categoryId = 0, string searchValue = "", decimal minPrice = 0, decimal maxPrice = 0, int page = 1, int pageSize = 10)
        {
            try
            {
                var input = new ProductSearchInput
                {
                    SearchValue = searchValue,
                    Page = page,
                    PageSize = pageSize,
                    CategoryID = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
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
                TempData["Error"] = "Không thể kết nối tới cơ sở dữ liệu. Vui lòng kiểm tra cấu hình và thử lại.";
                var empty = new PagedResult<Product> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<Product>() };
                ViewBag.Categories = new System.Collections.Generic.List<Category>();
                return View(empty);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(ProductSearchInput input)
        {
            try
            {
                var result = SV22T1020761.BusinessLayers.CatalogDataService.ListProducts(input);

                // Render only the product grid partial view
                return PartialView("_ProductGrid", result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ProductsController.Search error: {0}", ex.Message);

                // Return an empty product grid in case of error
                var empty = new PagedResult<Product> { Page = input.Page, PageSize = input.PageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<Product>() };
                return PartialView("_ProductGrid", empty);
            }
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
