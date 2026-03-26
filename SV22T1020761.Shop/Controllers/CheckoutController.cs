using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models.Sales;
using SV22T1020761.Shop.AppCodes;
using SV22T1020761.BusinessLayers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020761.Shop.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var summary = CartHelper.GetCartSummary(HttpContext.Session);
            ViewBag.Cart = cart;
            ViewBag.Summary = summary;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Place(string deliveryProvince, string deliveryAddress)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Gi? hàng r?ng";
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                CustomerID = null,
                CustomerName = User?.Identity?.Name ?? "",
                OrderTime = DateTime.Now,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                Status = OrderStatusEnum.New,
                TotalAmount = CartHelper.GetCartSummary(HttpContext.Session).Total
            };

            var details = new List<OrderDetail>();
            foreach (var it in cart)
            {
                details.Add(new OrderDetail
                {
                    ProductID = it.ProductID,
                    Quantity = it.Qty,
                    SalePrice = it.Price
                });
            }

            try
            {
                var orderId = await SalesDataService.AddOrderAsync(order, details);
                CartHelper.ClearCart(HttpContext.Session);
                TempData["Success"] = "Ð?t hàng thành công";
                return RedirectToAction("Details", "Orders", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không th? t?o ðõn hàng. Vui l?ng th? l?i.";
                return RedirectToAction("Index", "Checkout");
            }
        }
    }
}
