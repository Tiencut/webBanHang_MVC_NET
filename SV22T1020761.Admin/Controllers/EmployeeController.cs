using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models;

namespace SV22T1020761.Admin.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
                // Add employee to database
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public IActionResult Edit(int id)
        {
            // Get employee by id
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Update employee in database
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public IActionResult Delete(int id)
        {
            // Get employee by id
            return View();
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            // Delete employee from database
            return RedirectToAction("Index");
        }
    }
}
