using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.HR;

namespace SV22T1020761.Admin.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var pagedResult = HRDataService.ListEmployees(input);

            return View(pagedResult);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                HRDataService.AddEmployee(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public IActionResult Edit(int id)
        {
            var employee = HRDataService.GetEmployee(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                HRDataService.UpdateEmployee(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public IActionResult Delete(int id)
        {
            var employee = HRDataService.GetEmployee(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            HRDataService.DeleteEmployee(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Search(PaginationSearchInput input)
        {
            // Save search conditions to session
            ApplicationContext.SetSessionData("EmployeeSearchConditions", input);

            // Fetch data based on search conditions
            var result = HRDataService.ListEmployees(input);

            // Return partial view with updated data
            return PartialView("_EmployeeTable", result);
        }
    }
}
