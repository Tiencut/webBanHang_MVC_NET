using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Admin.AppCodes;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Sales;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SV22T1020761.Admin.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            try
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
            catch (System.Exception ex)
            {
                TempData["Error"] = "Không thể tải danh sách đơn hàng. Vui lòng thử lại sau.";
                return View(new PagedResult<Order> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new List<Order>() });
            }
        }

        public IActionResult Create()
        {
            try
            {
                // Lấy danh sách khách hàng
                var customersResult = PartnerDataService.ListCustomers(new PaginationSearchInput 
                { 
                    PageSize = 0,
                    Page = 1,
                    SearchValue = ""
                });
                ViewBag.Customers = new SelectList(customersResult.DataItems, "CustomerID", "CustomerName");

                // Lấy danh sách tỉnh thành
                var provinces = DictionaryDataService.ListProvinces(new PaginationSearchInput 
                { 
                    PageSize = 0,
                    Page = 1,
                    SearchValue = ""
                });
                ViewBag.Provinces = new SelectList(provinces.DataItems);

                // Lấy danh sách mặt hàng từ giỏ hàng (session)
                var cart = AppCodes.ShoppingCartService.GetShoppingCart();
                ViewBag.Cart = cart;

                return View(new Order());
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Không thể tải dữ liệu. Vui lòng thử lại sau: " + ex.Message;
                return View(new Order());
            }
        }

        [HttpPost]
        public IActionResult Create(Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SalesDataService.AddOrder(order);
                    return RedirectToAction("Index");
                }
                return View(order);
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Không thể tạo đơn hàng. Vui lòng thử lại sau.";
                return View(order);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var order = SalesDataService.GetOrder(id);
                if (order == null)
                {
                    return NotFound();
                }
                return View(order);
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Không thể tải đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SalesDataService.UpdateOrder(order);
                    return RedirectToAction("Index");
                }
                return View(order);
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Không thể cập nhật đơn hàng. Vui lòng thử lại sau.";
                return View(order);
            }
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

        /// <summary>
        /// Xem giỏ hàng
        /// </summary>
        [HttpGet]
        public IActionResult Cart()
        {
            var cart = AppCodes.ShoppingCartService.GetShoppingCart();
            return View(cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult AddToCart(int productID, int quantity)
        {
            try
            {
                var product = CatalogDataService.GetProduct(productID);
                if (product == null)
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });

                System.Diagnostics.Debug.WriteLine($"=== ADD TO CART DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"ProductID: {productID}");
                System.Diagnostics.Debug.WriteLine($"Product from DB: {product.ProductName}, Price: {product.Price}, Unit: {product.Unit}, Photo: {product.Photo}");

                var item = new OrderDetailViewInfo
                {
                    ProductID = productID,
                    ProductName = product.ProductName,
                    Quantity = quantity,
                    SalePrice = product.Price,
                    Unit = product.Unit,
                    Photo = product.Photo
                };

                System.Diagnostics.Debug.WriteLine($"Item before AddCartItem: ProductName={item.ProductName}, Quantity={item.Quantity}, SalePrice={item.SalePrice}, Unit={item.Unit}");

                AppCodes.ShoppingCartService.AddCartItem(item);

                // Verify what's in cart after add
                var cart = AppCodes.ShoppingCartService.GetShoppingCart();
                System.Diagnostics.Debug.WriteLine($"Cart now has {cart.Count} items");
                foreach (var cartItem in cart)
                {
                    System.Diagnostics.Debug.WriteLine($"  - ProductID: {cartItem.ProductID}, ProductName: {cartItem.ProductName}, Qty: {cartItem.Quantity}, Price: {cartItem.SalePrice}");
                }
                System.Diagnostics.Debug.WriteLine($"=== END DEBUG ===");

                return Json(new { success = true, message = "Thêm vào giỏ hàng thành công" });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in AddToCart: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi khi thêm vào giỏ hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult UpdateCart(int productID, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    AppCodes.ShoppingCartService.RemoveCartItem(productID);
                    return Json(new { success = true, message = "Xóa khỏi giỏ hàng" });
                }

                var item = AppCodes.ShoppingCartService.GetCartItem(productID);
                if (item == null)
                    return NotFound();

                AppCodes.ShoppingCartService.UpdateCartItem(productID, quantity, item.SalePrice);
                return Json(new { success = true, message = "Cập nhật giỏ hàng thành công" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult RemoveFromCart(int productID)
        {
            try
            {
                AppCodes.ShoppingCartService.RemoveCartItem(productID);
                return Json(new { success = true, message = "Xóa khỏi giỏ hàng thành công" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo đơn hàng từ giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult CreateOrderFromCart(Order order)
        {
            try
            {
                var cart = AppCodes.ShoppingCartService.GetShoppingCart();
                if (cart == null || cart.Count == 0)
                    return Json(new { success = false, message = "Giỏ hàng trống" });

                // Tạo đơn hàng
                order.OrderTime = DateTime.Now;
                SalesDataService.AddOrder(order);

                // Lấy ID đơn hàng vừa tạo
                var latestOrder = SalesDataService.GetLatestOrder();
                if (latestOrder != null)
                {
                    // Thêm các sản phẩm từ giỏ vào chi tiết đơn hàng
                    foreach (var item in cart)
                    {
                        var detail = new OrderDetail
                        {
                            OrderID = latestOrder.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            SalePrice = item.SalePrice
                        };
                        SalesDataService.AddOrderDetail(detail);
                    }

                    // Xóa giỏ hàng
                    AppCodes.ShoppingCartService.ClearCart();

                    return Json(new { success = true, message = "Tạo đơn hàng thành công", orderID = latestOrder.OrderID });
                }

                return Json(new { success = false, message = "Không thể tạo đơn hàng" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
                AppCodes.ShoppingCartService.ClearCart();
                return Json(new { success = true, message = "Giỏ hàng đã được xóa" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm sản phẩm (AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult SearchProducts(string searchValue = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var input = new PaginationSearchInput
                {
                    SearchValue = searchValue,
                    Page = page,
                    PageSize = pageSize
                };

                var products = CatalogDataService.ListProducts(input);
                return PartialView("_ProductList", products);
            }
            catch (System.Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        /// <summary>
        /// Lấy HTML giỏ hàng cập nhật
        /// </summary>
        [HttpGet]
        public IActionResult GetCartTable()
        {
            var cart = AppCodes.ShoppingCartService.GetShoppingCart();
            // Debug: Log cart data
            foreach (var item in cart)
            {
                System.Diagnostics.Debug.WriteLine($"Cart Item - ProductID: {item.ProductID}, ProductName: {item.ProductName ?? "NULL"}, Quantity: {item.Quantity}, SalePrice: {item.SalePrice}, Unit: {item.Unit ?? "NULL"}");
            }
            return PartialView("_CartTable", cart);
        }

        /// <summary>
        /// Hiển thị form sửa số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpGet]
        public IActionResult EditCartItem(int productID)
        {
            var item = AppCodes.ShoppingCartService.GetCartItem(productID);
            if (item == null)
                return NotFound();
            
            return PartialView("_EditCartItem", item);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult EditCartItem(int productID, int quantity, decimal salePrice)
        {
            try
            {
                if (quantity <= 0)
                    return BadRequest("Số lượng phải lớn hơn 0");

                AppCodes.ShoppingCartService.UpdateCartItem(productID, quantity, salePrice);
                return Json(new { success = true, message = "Cập nhật giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Hiển thị dialog xác nhận xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpGet]
        public IActionResult DeleteCartItem(int productID)
        {
            var item = AppCodes.ShoppingCartService.GetCartItem(productID);
            if (item == null)
                return NotFound();
            
            return PartialView("_DeleteCartItem", item);
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult DeleteCartItem(int productID, string confirm)
        {
            try
            {
                AppCodes.ShoppingCartService.RemoveCartItem(productID);
                return Json(new { success = true, message = "Xóa khỏi giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}