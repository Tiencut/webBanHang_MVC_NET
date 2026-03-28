using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Shop.Models;
using System.Diagnostics;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;

namespace SV22T1020761.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Load a small list of featured products for home page
            try
            {
                var input = new PaginationSearchInput { Page = 1, PageSize = 8 };
                var model = SV22T1020761.BusinessLayers.CatalogDataService.ListProducts(input);

                // cart summary via session helper
                var summary = SV22T1020761.Shop.AppCodes.CartHelper.GetCartSummary(HttpContext.Session);
                ViewBag.CartCount = summary.Count;

                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading home featured products");
                TempData["Error"] = "Không th? t?i danh sách s?n ph?m. Vui l?ng th? l?i sau.";
                var empty = new PagedResult<Product> { Page = 1, PageSize = 8, RowCount = 0, DataItems = new System.Collections.Generic.List<Product>() };
                ViewBag.CartCount = 0;
                return View(empty);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
