using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Shop.AppCodes;
using Microsoft.AspNetCore.Http;

namespace SV22T1020761.Shop.Controllers
{
    public class CartController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int qty = 1)
        {
            // get product price/name via CatalogDataService
            var product = SV22T1020761.BusinessLayers.CatalogDataService.GetProduct(productId);
            if (product == null) return BadRequest();

            var item = new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Qty = qty,
                Photo = product.Photo
            };

            CartHelper.AddToCart(HttpContext.Session, item);

            var summary = CartHelper.GetCartSummary(HttpContext.Session);
            return PartialView("~/Views/Shared/_CartSummary.cshtml", summary);
        }

        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int qty)
        {
            CartHelper.UpdateCartItem(HttpContext.Session, productId, qty);
            var cart = CartHelper.GetCart(HttpContext.Session);
            return PartialView("~/Views/Cart/_CartTable.cshtml", cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            CartHelper.RemoveFromCart(HttpContext.Session, productId);
            var cart = CartHelper.GetCart(HttpContext.Session);
            return PartialView("~/Views/Cart/_CartTable.cshtml", cart);
        }
    }
}
