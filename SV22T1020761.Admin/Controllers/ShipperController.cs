using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;

namespace SV22T1020761.Admin.Controllers
{
    public class ShipperController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                PartnerDataService.AddShipper(shipper);
                return RedirectToAction("Index");
            }
            return View(shipper);
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
        public IActionResult Edit(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                PartnerDataService.UpdateShipper(shipper);
                return RedirectToAction("Index");
            }
            return View(shipper);
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
        public IActionResult DeleteConfirmed(int id)
        {
            PartnerDataService.DeleteShipper(id);
            return RedirectToAction("Index");
        }
    }
}
