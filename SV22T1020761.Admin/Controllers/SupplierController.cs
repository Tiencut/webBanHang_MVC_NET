using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models;

namespace SV22T1020761.Admin.Controllers
{
    public class SupplierController : Controller
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
        public IActionResult Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                // Add supplier to database
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        public IActionResult Edit(int id)
        {
            // Get supplier by id
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                // Update supplier in database
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        public IActionResult Delete(int id)
        {
            // Get supplier by id
            return View();
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            // Delete supplier from database
            return RedirectToAction("Index");
        }
    }
}

