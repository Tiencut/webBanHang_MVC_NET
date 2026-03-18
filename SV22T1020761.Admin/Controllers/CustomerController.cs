using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;

namespace SV22T1020761.Admin.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index(PaginationSearchInput input)
        {
            var model = PartnerDataService.ListCustomers(input);
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(SV22T1020761.Models.Partner.Customer customer)
        {
            if (ModelState.IsValid)
            {
                PartnerDataService.AddCustomer(customer);
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
        public IActionResult Edit(SV22T1020761.Models.Partner.Customer customer)
        {
            if (ModelState.IsValid)
            {
                PartnerDataService.UpdateCustomer(customer);
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

        [HttpPost]
        public IActionResult Search(PaginationSearchInput input)
        {
            ApplicationContext.SetSessionData("CustomerSearchConditions", input);
            var result = CustomerService.ListCustomers(input);
            return PartialView("_CustomerTable", result);
        }
    }
}
