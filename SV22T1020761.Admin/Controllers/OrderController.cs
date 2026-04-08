using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Admin.AppCodes;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Sales;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020761.Admin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10, string status = "", string dateFrom = "", string dateTo = "")
        {
            try
            {
                var input = new OrderSearchInput
                {
                    SearchValue = searchValue ?? "",
                    Page = page,
                    PageSize = pageSize,
                    Status = string.IsNullOrWhiteSpace(status) ? OrderStatusEnum.All : (OrderStatusEnum)int.Parse(status)
                };

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
                {
                    input.DateFrom = fromDate;
                }
                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
                {
                    input.DateTo = toDate.AddDays(1);
                }

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
                        CustomerName = order.CustomerName,
                        OrderTime = order.OrderTime,
                        OrderDate = order.OrderTime,  // Copy OrderTime to OrderDate
                        DeliveryProvince = order.DeliveryProvince,
                        DeliveryAddress = order.DeliveryAddress,
                        EmployeeID = order.EmployeeID,
                        AcceptTime = order.AcceptTime,
                        ShipperID = order.ShipperID,
                        ShippedTime = order.ShippedTime,
                        FinishedTime = order.FinishedTime,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount  // Copy TotalAmount
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

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Get order details
                var orderDetails = await SalesDataService.ListOrderDetailsAsync(id);
                ViewBag.OrderDetails = orderDetails;

                return View(order);
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Không thể tải chi tiết đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Create()
        {
            try
            {
                Console.WriteLine("=== CREATE GET DEBUG ===");
                
                // Lấy danh sách khách hàng
                var customersResult = PartnerDataService.ListCustomers(new PaginationSearchInput 
                { 
                    PageSize = 0,
                    Page = 1,
                    SearchValue = ""
                });
                Console.WriteLine($"Customers loaded: {customersResult.DataItems.Count}");
                ViewBag.Customers = new SelectList(customersResult.DataItems, "CustomerID", "CustomerName");

                // Lấy danh sách tỉnh thành
                var provinces = DictionaryDataService.ListProvinces(new PaginationSearchInput 
                { 
                    PageSize = 0,
                    Page = 1,
                    SearchValue = ""
                });
                Console.WriteLine($"Provinces loaded: {provinces.DataItems.Count}");
                foreach (var prov in provinces.DataItems)
                {
                    Console.WriteLine($"  - {prov}");
                }
                ViewBag.Provinces = new SelectList(provinces.DataItems);

                // Lấy danh sách mặt hàng từ giỏ hàng (session)
                var cart = AppCodes.ShoppingCartService.GetShoppingCart();
                ViewBag.Cart = cart;

                return View(new Order());
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"ERROR in Create GET: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["Error"] = "Không thể tải dữ liệu. Vui lòng thử lại sau: " + ex.Message;
                return View(new Order());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order, string cartData)
        {
            try
            {
                Console.WriteLine("=== CREATE ORDER DEBUG ===");
                Console.WriteLine($"CartData received: {cartData}");
                Console.WriteLine($"CustomerID: {order.CustomerID}");
                Console.WriteLine($"DeliveryProvince: {order.DeliveryProvince}");
                
                if (string.IsNullOrWhiteSpace(cartData))
                {
                    Console.WriteLine("ERROR: CartData is empty");
                    ModelState.AddModelError("", "Vui lòng thêm sản phẩm vào giỏ hàng");
                    return View(order);
                }

                // Parse cart data from JSON
                using (var doc = System.Text.Json.JsonDocument.Parse(cartData))
                {
                    var root = doc.RootElement;
                    Console.WriteLine($"Cart items count: {root.GetArrayLength()}");
                    
                    if (root.GetArrayLength() == 0)
                    {
                        Console.WriteLine("ERROR: Cart is empty");
                        ModelState.AddModelError("", "Giỏ hàng không có sản phẩm");
                        return View(order);
                    }

                    // Calculate total amount and build order details
                    decimal totalAmount = 0;
                    var orderDetails = new List<OrderDetail>();

                    foreach (var item in root.EnumerateArray())
                    {
                        if (!item.TryGetProperty("productId", out var productIdElem) ||
                            !item.TryGetProperty("quantity", out var quantityElem))
                        {
                            Console.WriteLine("ERROR: Missing productId or quantity");
                            continue;
                        }

                        int productId = productIdElem.GetInt32();
                        int quantity = quantityElem.GetInt32();
                        Console.WriteLine($"Processing product: ID={productId}, Qty={quantity}");

                        var product = CatalogDataService.GetProduct(productId);
                        if (product == null)
                        {
                            Console.WriteLine($"ERROR: Product {productId} not found");
                            ModelState.AddModelError("", $"Sản phẩm ID {productId} không tồn tại");
                            return View(order);
                        }

                        var lineTotal = product.Price * quantity;
                        totalAmount += lineTotal;

                        orderDetails.Add(new OrderDetail
                        {
                            ProductID = productId,
                            Quantity = quantity,
                            SalePrice = product.Price
                        });
                    }

                    if (orderDetails.Count == 0)
                    {
                        Console.WriteLine("ERROR: No valid order details");
                        ModelState.AddModelError("", "Giỏ hàng không có sản phẩm hợp lệ");
                        return View(order);
                    }

                    // Validate required fields
                    if (order.CustomerID <= 0)
                    {
                        Console.WriteLine($"ERROR: Invalid CustomerID={order.CustomerID}");
                        ModelState.AddModelError("CustomerID", "Vui lòng chọn khách hàng");
                        return View(order);
                    }

                    if (string.IsNullOrWhiteSpace(order.DeliveryProvince))
                    {
                        Console.WriteLine("ERROR: DeliveryProvince is empty");
                        ModelState.AddModelError("DeliveryProvince", "Vui lòng chọn tỉnh/thành");
                        return View(order);
                    }

                    // Set order properties
                    var orderRequest = new OrderCreateRequest
                    {
                        CustomerID = order.CustomerID,
                        OrderTime = DateTime.Now,
                        DeliveryProvince = order.DeliveryProvince,
                        DeliveryAddress = order.DeliveryAddress,
                        Status = OrderStatusEnum.New,
                        EmployeeID = null,
                        AcceptTime = null,
                        ShipperID = null,
                        ShippedTime = null,
                        FinishedTime = null
                    };

                    Console.WriteLine($"Creating order with {orderDetails.Count} items, Total={totalAmount}");
                    
                    // Create order and order details
                    int orderId = await SalesDataService.AddOrderAsync(orderRequest, orderDetails);
                    Console.WriteLine($"Order created successfully: ID={orderId}");
                    
                    if (orderId > 0)
                    {
                        TempData["Success"] = $"Tạo đơn hàng #{orderId} thành công!";
                        return RedirectToAction("Details", new { id = orderId });
                    }
                    else
                    {
                        Console.WriteLine("ERROR: orderId <= 0");
                        ModelState.AddModelError("", "Không thể tạo đơn hàng. Vui lòng thử lại.");
                        return View(order);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["Error"] = "Không thể tạo đơn hàng: " + ex.Message;
                return View(order);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Convert OrderViewInfo to Order for the form
                var formModel = new Order
                {
                    OrderID = order.OrderID,
                    CustomerID = order.CustomerID,
                    CustomerName = order.CustomerName,
                    OrderTime = order.OrderTime,
                    OrderDate = order.OrderTime,
                    DeliveryProvince = order.DeliveryProvince,
                    DeliveryAddress = order.DeliveryAddress,
                    EmployeeID = order.EmployeeID,
                    AcceptTime = order.AcceptTime,
                    ShipperID = order.ShipperID,
                    ShippedTime = order.ShippedTime,
                    FinishedTime = order.FinishedTime,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount
                };

                return View(formModel);
            }
            catch (System.Exception)
            {
                TempData["Error"] = "Không thể tải đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string action)
        {
            try
            {
                var orderView = await SalesDataService.GetOrderAsync(id);
                if (orderView == null)
                {
                    return NotFound();
                }

                // Convert to OrderCreateRequest for update
                var orderRequest = new OrderCreateRequest
                {
                    CustomerID = orderView.CustomerID,
                    OrderTime = orderView.OrderTime,
                    DeliveryProvince = orderView.DeliveryProvince,
                    DeliveryAddress = orderView.DeliveryAddress,
                    EmployeeID = orderView.EmployeeID,
                    AcceptTime = orderView.AcceptTime,
                    ShipperID = orderView.ShipperID,
                    ShippedTime = orderView.ShippedTime,
                    FinishedTime = orderView.FinishedTime,
                    Status = orderView.Status
                };

                // Update order status based on action
                switch (action)
                {
                    case "New":
                        orderRequest.Status = OrderStatusEnum.New;
                        break;
                    case "Accept":
                        if (orderRequest.Status != OrderStatusEnum.New) throw new InvalidOperationException("Chỉ có thể duyệt đơn mới");
                        orderRequest.Status = OrderStatusEnum.Accepted;
                        orderRequest.AcceptTime = DateTime.Now;
                        break;
                    case "Ship":
                        if (orderRequest.Status != OrderStatusEnum.Accepted) throw new InvalidOperationException("Chỉ có thể chuyển đơn đã duyệt");
                        orderRequest.Status = OrderStatusEnum.Shipping;
                        orderRequest.ShippedTime = DateTime.Now;
                        break;
                    case "Complete":
                        if (orderRequest.Status != OrderStatusEnum.Shipping) throw new InvalidOperationException("Chỉ có thể hoàn tất đơn đang giao");
                        orderRequest.Status = OrderStatusEnum.Completed;
                        orderRequest.FinishedTime = DateTime.Now;
                        break;
                    case "Reject":
                        if (orderRequest.Status != OrderStatusEnum.New) throw new InvalidOperationException("Chỉ có thể từ chối đơn mới");
                        orderRequest.Status = OrderStatusEnum.Rejected;
                        break;
                    case "Cancel":
                        if (orderRequest.Status == OrderStatusEnum.Completed || orderRequest.Status == OrderStatusEnum.Cancelled || orderRequest.Status == OrderStatusEnum.Rejected)
                            throw new InvalidOperationException("Không thể hủy đơn đã hoàn tất/hủy/từ chối");
                        orderRequest.Status = OrderStatusEnum.Cancelled;
                        break;
                }

                await SalesDataService.UpdateOrderAsync(orderRequest, id);
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công.";
                return RedirectToAction("Details", new { id });
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message ?? "Không thể cập nhật đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Edit", new { id });
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