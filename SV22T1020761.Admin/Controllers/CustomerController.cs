using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SV22T1020761.Admin.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(PaginationSearchInput input)
        {
            try
            {
                var model = PartnerDataService.ListCustomers(input);
                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading customers");
                TempData["Error"] = "Không thể kết nối tới cơ sở dữ liệu. Vui lòng kiểm tra cấu hình và thử lại.";
                var empty = new PagedResult<Customer> { Page = input?.Page ?? 1, PageSize = input?.PageSize ?? 10, RowCount = 0, DataItems = new System.Collections.Generic.List<Customer>() };
                return View(empty);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Form(int? id, bool delete = false)
        {
            if (id == null || id == 0)
            {
                var model = new Customer();
                if (delete) return BadRequest();
                return PartialView("_CustomerForm", model);
            }
            var customer = await PartnerDataService.GetCustomerAsync(id.Value);
            if (customer == null) return NotFound();
            if (delete) return PartialView("_CustomerDelete", customer);
            return PartialView("_CustomerForm", customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();

                await PartnerDataService.AddCustomerAsync(customer);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = PartnerDataService.ListCustomers(input);
                    return PartialView("_CustomerTable", result);
                }

                TempData["Success"] = "Thêm khách hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating customer");
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(customer);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                await PartnerDataService.UpdateCustomerAsync(customer);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = PartnerDataService.ListCustomers(input);
                    return PartialView("_CustomerTable", result);
                }
                TempData["Success"] = "Cập nhật khách hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating customer (Id={CustomerId})", customer?.CustomerID);
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(customer);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await PartnerDataService.DeleteCustomerAsync(id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = PartnerDataService.ListCustomers(input);
                    return PartialView("_CustomerTable", result);
                }
                TempData["Success"] = "Xóa khách hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting customer (Id={CustomerId})", id);
                var customer = await PartnerDataService.GetCustomerAsync(id);
                ModelState.AddModelError(string.Empty, "Không thể xóa khách hàng. Vui lòng thử lại sau.");
                return View("Delete", customer);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(PaginationSearchInput input)
        {
            ApplicationContext.SetSessionData("CustomerSearchConditions", input);
            var result = PartnerDataService.ListCustomers(input);
            return PartialView("_CustomerTable", result);
        }
    }
}
