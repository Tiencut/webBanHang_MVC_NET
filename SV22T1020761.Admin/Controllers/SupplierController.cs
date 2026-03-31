using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace SV22T1020761.Admin.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(ILogger<SupplierController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            try
            {
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
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading suppliers");
                TempData["Error"] = "Không thể kết nối tới cơ sở dữ liệu. Vui lòng kiểm tra cấu hình và thử lại.";
                var empty = new PagedResult<SV22T1020761.Models.Supplier> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<SV22T1020761.Models.Supplier>() };
                ViewBag.PageNumber = empty.Page;
                ViewBag.TotalPages = empty.PageCount;
                return View(empty);
            }
        }

        [HttpGet]
        public IActionResult Form(int? id, bool delete = false)
        {
            if (id == null || id == 0)
            {
                var model = new Supplier();
                if (delete) return BadRequest();
                return PartialView("_SupplierForm", model);
            }
            var supplier = PartnerDataService.GetSupplier(id.Value);
            if (supplier == null) return NotFound();
            if (delete) return PartialView("_SupplierDelete", supplier);
            return PartialView("_SupplierForm", supplier);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Supplier supplier)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                PartnerDataService.AddSupplier(supplier);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var paged = PartnerDataService.ListSuppliers(input);
                    var mapped = new PagedResult<SV22T1020761.Models.Supplier>
                    {
                        Page = paged.Page,
                        PageSize = paged.PageSize,
                        RowCount = paged.RowCount,
                        DataItems = paged.DataItems.Select(s => new SV22T1020761.Models.Supplier { SupplierID = s.SupplierID, SupplierName = s.SupplierName, ContactName = s.ContactName, Province = s.Province, Address = s.Address }).ToList()
                    };
                    return PartialView("_SupplierTable", mapped);
                }

                TempData["Success"] = "Thêm nhà cung cấp thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating supplier");
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(supplier);
            }
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Supplier supplier)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                PartnerDataService.UpdateSupplier(supplier);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var paged = PartnerDataService.ListSuppliers(input);
                    var mapped = new PagedResult<SV22T1020761.Models.Supplier>
                    {
                        Page = paged.Page,
                        PageSize = paged.PageSize,
                        RowCount = paged.RowCount,
                        DataItems = paged.DataItems.Select(s => new SV22T1020761.Models.Supplier { SupplierID = s.SupplierID, SupplierName = s.SupplierName, ContactName = s.ContactName, Province = s.Province, Address = s.Address }).ToList()
                    };
                    return PartialView("_SupplierTable", mapped);
                }

                TempData["Success"] = "Cập nhật nhà cung cấp thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating supplier (Id={SupplierId})", supplier?.SupplierID);
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(supplier);
            }
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
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                PartnerDataService.DeleteSupplier(id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var paged = PartnerDataService.ListSuppliers(input);
                    var mapped = new PagedResult<SV22T1020761.Models.Supplier>
                    {
                        Page = paged.Page,
                        PageSize = paged.PageSize,
                        RowCount = paged.RowCount,
                        DataItems = paged.DataItems.Select(s => new SV22T1020761.Models.Supplier { SupplierID = s.SupplierID, SupplierName = s.SupplierName, ContactName = s.ContactName, Province = s.Province, Address = s.Address }).ToList()
                    };
                    return PartialView("_SupplierTable", mapped);
                }

                TempData["Success"] = "Xóa nhà cung cấp thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting supplier (Id={SupplierId})", id);
                var supplier = PartnerDataService.GetSupplier(id);
                ModelState.AddModelError(string.Empty, "Không thể xóa nhà cung cấp. Vui lòng thử lại sau.");
                return View("Delete", supplier);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

