using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models;

namespace SV22T1020761.Admin.Controllers
{
    public class ShipperController : Controller
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
        public IActionResult Create(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                // Add shipper to database
                return RedirectToAction("Index");
            }
            return View(shipper);
        }

        public IActionResult Edit(int id)
        {
            // Get shipper by id
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                // Update shipper in database
                return RedirectToAction("Index");
            }
            return View(shipper);
        }

        public IActionResult Delete(int id)
        {
            // Get shipper by id
            return View();
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            // Delete shipper from database
            return RedirectToAction("Index");
        }
    }
}
