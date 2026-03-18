using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;

namespace SV22T1020761.Admin.Controllers
{
    public class SupplierController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var pagedResult = PartnerDataService.ListSuppliers(input);

            // Map Partner.Supplier to Models.Supplier
            var mappedResult = new PagedResult<SV22T1020761.Models.Supplier>
            {
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                RowCount = pagedResult.RowCount,
                DataItems = pagedResult.DataItems.Select(s => new SV22T1020761.Models.Supplier
                {
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    Province = s.Province,
                    Address = s.Address
                }).ToList()
            };

            ViewBag.PageNumber = mappedResult.Page;
            ViewBag.TotalPages = mappedResult.PageCount;
            return View(mappedResult);
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
                PartnerDataService.AddSupplier(supplier);
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        public IActionResult Edit(int id)
        {
            var supplier = PartnerDataService.GetSupplier(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        [HttpPost]
        public IActionResult Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                PartnerDataService.UpdateSupplier(supplier);
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        public IActionResult Delete(int id)
        {
            var supplier = PartnerDataService.GetSupplier(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            PartnerDataService.DeleteSupplier(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Search(PaginationSearchInput input)
        {
            // Save search conditions to session
            ApplicationContext.SetSessionData("SupplierSearchConditions", input);

            // Fetch data based on search conditions
            var pagedResult = PartnerDataService.ListSuppliers(input);

            // Map Partner.Supplier to Models.Supplier
            var mappedResult = new PagedResult<SV22T1020761.Models.Supplier>
            {
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                RowCount = pagedResult.RowCount,
                DataItems = pagedResult.DataItems.Select(s => new SV22T1020761.Models.Supplier
                {
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    Province = s.Province,
                    Address = s.Address
                }).ToList()
            };

            // Return partial view with updated data
            return PartialView("_SupplierTable", mappedResult);
        }
    }
}

