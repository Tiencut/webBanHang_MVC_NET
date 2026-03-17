using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;

namespace SV22T1020761.Admin.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var model = CustomerService.ListCustomers(input);
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                CustomerService.AddCustomer(customer);
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public IActionResult Edit(int id)
        {
            var customer = CustomerService.GetCustomer(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                CustomerService.UpdateCustomer(customer);
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public IActionResult Delete(int id)
        {
            var customer = CustomerService.GetCustomer(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            CustomerService.DeleteCustomer(id);
            return RedirectToAction("Index");
        }
    }
}
