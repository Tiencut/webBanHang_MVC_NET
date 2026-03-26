using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Sales;
using SV22T1020761.BusinessLayers;
using System.Threading.Tasks;

namespace SV22T1020761.Shop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput { Page = page, PageSize = pageSize };
            var model = SalesDataService.ListOrders(input);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return NotFound();
            var details = await SalesDataService.ListOrderDetailsAsync(id);
            ViewBag.Details = details;
            return View(order);
        }
    }
}
