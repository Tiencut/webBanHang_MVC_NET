using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;
using Microsoft.Extensions.Logging;

namespace SV22T1020761.Admin.Controllers
{
    public class ShipperController : Controller
    {
        private readonly ILogger<ShipperController> _logger;

        public ShipperController(ILogger<ShipperController> logger)
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
                var pagedResult = PartnerDataService.ListShippers(input);
                var model = new PagedResult<SV22T1020761.Models.Shipper>
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    RowCount = pagedResult.RowCount,
                    DataItems = pagedResult.DataItems.Select(shipper => new SV22T1020761.Models.Shipper
                    {
                        ShipperID = shipper.ShipperID,
                        ShipperName = shipper.ShipperName,
                        Phone = shipper.Phone
                    }).ToList()
                };

                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading shippers");
                TempData["Error"] = "Không th? k?t n?i t?i cõ s? d? li?u. Vui l?ng ki?m tra c?u h?nh và th? l?i.";
                var empty = new PagedResult<SV22T1020761.Models.Shipper> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<SV22T1020761.Models.Shipper>() };
                return View(empty);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(PaginationSearchInput input)
        {
            var pagedResult = PartnerDataService.ListShippers(input);
            var model = new PagedResult<SV22T1020761.Models.Shipper>
            {
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                RowCount = pagedResult.RowCount,
                DataItems = pagedResult.DataItems.Select(shipper => new SV22T1020761.Models.Shipper
                {
                    ShipperID = shipper.ShipperID,
                    ShipperName = shipper.ShipperName,
                    Phone = shipper.Phone
                }).ToList()
            };
            return PartialView("_ShipperTable", model);
        }

        [HttpGet]
        public IActionResult Form(int? id, bool delete = false)
        {
            if (id == null || id == 0)
            {
                var model = new Shipper();
                if (delete) return BadRequest();
                return PartialView("_ShipperForm", model);
            }
            var shipper = PartnerDataService.GetShipper(id.Value);
            if (shipper == null) return NotFound();
            if (delete) return PartialView("_ShipperDelete", shipper);
            return PartialView("_ShipperForm", shipper);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Shipper shipper)
        {
            try
            {
                if (!ModelState.IsValid) return View(shipper);
                PartnerDataService.AddShipper(shipper);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var pagedResult = PartnerDataService.ListShippers(input);
                    var model = new PagedResult<SV22T1020761.Models.Shipper>
                    {
                        Page = pagedResult.Page,
                        PageSize = pagedResult.PageSize,
                        RowCount = pagedResult.RowCount,
                        DataItems = pagedResult.DataItems.Select(s => new SV22T1020761.Models.Shipper { ShipperID = s.ShipperID, ShipperName = s.ShipperName, Phone = s.Phone }).ToList()
                    };
                    return PartialView("_ShipperTable", model);
                }
                TempData["Success"] = "Thêm ð?i tác giao hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating shipper");
                ModelState.AddModelError(string.Empty, "H? th?ng ðang b?n. Vui l?ng th? l?i sau.");
                return View(shipper);
            }
        }

        public IActionResult Edit(int id)
        {
            var shipper = PartnerDataService.GetShipper(id);
            if (shipper == null)
            {
                return NotFound();
            }
            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Shipper shipper)
        {
            try
            {
                if (!ModelState.IsValid) return View(shipper);
                PartnerDataService.UpdateShipper(shipper);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var pagedResult = PartnerDataService.ListShippers(input);
                    var model = new PagedResult<SV22T1020761.Models.Shipper>
                    {
                        Page = pagedResult.Page,
                        PageSize = pagedResult.PageSize,
                        RowCount = pagedResult.RowCount,
                        DataItems = pagedResult.DataItems.Select(s => new SV22T1020761.Models.Shipper { ShipperID = s.ShipperID, ShipperName = s.ShipperName, Phone = s.Phone }).ToList()
                    };
                    return PartialView("_ShipperTable", model);
                }
                TempData["Success"] = "C?p nh?t ð?i tác giao hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating shipper (Id={ShipperId})", shipper?.ShipperID);
                ModelState.AddModelError(string.Empty, "H? th?ng ðang b?n. Vui l?ng th? l?i sau.");
                return View(shipper);
            }
        }

        public IActionResult Delete(int id)
        {
            var shipper = PartnerDataService.GetShipper(id);
            if (shipper == null)
            {
                return NotFound();
            }
            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                PartnerDataService.DeleteShipper(id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var pagedResult = PartnerDataService.ListShippers(input);
                    var model = new PagedResult<SV22T1020761.Models.Shipper>
                    {
                        Page = pagedResult.Page,
                        PageSize = pagedResult.PageSize,
                        RowCount = pagedResult.RowCount,
                        DataItems = pagedResult.DataItems.Select(s => new SV22T1020761.Models.Shipper { ShipperID = s.ShipperID, ShipperName = s.ShipperName, Phone = s.Phone }).ToList()
                    };
                    return PartialView("_ShipperTable", model);
                }
                TempData["Success"] = "Xóa ð?i tác giao hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting shipper (Id={ShipperId})", id);
                var shipper = PartnerDataService.GetShipper(id);
                ModelState.AddModelError(string.Empty, "Không th? xóa ð?i tác giao hàng. Vui l?ng th? l?i sau.");
                return View("Delete", shipper);
            }
        }
    }
}
