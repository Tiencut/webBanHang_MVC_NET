using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Sales;

namespace SV22T1020761.Admin.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var orders = SalesDataService.ListOrders(input);
            var model = new PagedResult<Order>
            {
                Page = orders.Page,
                PageSize = orders.PageSize,
                RowCount = orders.RowCount,
                DataItems = orders.DataItems.Select(order => new Order
                {
                    OrderID = order.OrderID,
                    CustomerID = order.CustomerID,
                    CustomerName = SalesDataService.GetCustomerName(order.CustomerID),
                    OrderTime = order.OrderTime,
                    DeliveryProvince = order.DeliveryProvince,
                    DeliveryAddress = order.DeliveryAddress,
                    EmployeeID = order.EmployeeID,
                    AcceptTime = order.AcceptTime,
                    ShipperID = order.ShipperID,
                    ShippedTime = order.ShippedTime,
                    FinishedTime = order.FinishedTime,
                    Status = order.Status
                }).ToList()
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                SalesDataService.AddOrder(order);
                return RedirectToAction("Index");
            }
            return View(order);
        }

        public IActionResult Edit(int id)
        {
            var order = SalesDataService.GetOrder(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public IActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                SalesDataService.UpdateOrder(order);
                return RedirectToAction("Index");
            }
            return View(order);
        }

        public IActionResult Delete(int id)
        {
            var order = SalesDataService.GetOrder(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            SalesDataService.DeleteOrder(id);
            return RedirectToAction("Index");
        }
    }
}