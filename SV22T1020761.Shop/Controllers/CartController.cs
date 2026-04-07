using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SV22T1020761.Shop.AppCodes;
using SV22T1020761.DataLayers.SQLServer.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020761.Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;

        public CartController(ILogger<CartController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int qty = 1)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Cart.Add called: productId={productId}, qty={qty}");
            
            var product = SV22T1020761.BusinessLayers.CatalogDataService.GetProduct(productId);
            System.Diagnostics.Debug.WriteLine($"GetProduct result: {(product == null ? "NULL" : product.ProductName)}");
            
            if (product == null)
            {
                System.Diagnostics.Debug.WriteLine("Returning BadRequest: product is null");
                return BadRequest(new { error = "Product not found" });
            }

            var username = User?.Identity?.Name ?? "";
            System.Diagnostics.Debug.WriteLine($"Username: '{username}'");
            
            var ua = SV22T1020761.Shop.Services.AccountService.GetUser(username);
            System.Diagnostics.Debug.WriteLine($"User account: {(ua == null ? "NULL" : ua.UserName)}");
            
            if (ua == null || !int.TryParse(ua.UserId, out var customerId))
            {
                // User not logged in - save pending product to session
                HttpContext.Session.SetInt32("PendingProductId", productId);
                HttpContext.Session.SetInt32("PendingProductQty", qty);
                System.Diagnostics.Debug.WriteLine("Returning requiresLogin - saved to session");
                return Json(new { requiresLogin = true });
            }

            try
            {
                var repo = new OrderRepository(SV22T1020761.BusinessLayers.Configuration.ConnectionString);
                
                // Get existing draft order for this customer
                var orders = await Task.Run(() => SV22T1020761.BusinessLayers.SalesDataService.ListOrders(
                    new SV22T1020761.Models.Sales.OrderSearchInput 
                    { 
                        Page = 1, 
                        PageSize = 100,
                        Status = SV22T1020761.Models.Sales.OrderStatusEnum.New,
                        CustomerID = customerId 
                    }));
                
                var draftOrder = orders?.DataItems?.FirstOrDefault(o => 
                    o.Status == SV22T1020761.Models.Sales.OrderStatusEnum.New);
                
                System.Diagnostics.Debug.WriteLine($"Draft order found: {(draftOrder == null ? "NO" : "YES, OrderID=" + draftOrder.OrderID)}");
                
                if (draftOrder == null)
                {
                    // Create new Draft order
                    var newOrder = new SV22T1020761.Models.Sales.Order
                    {
                        CustomerID = customerId,
                        OrderTime = DateTime.Now,
                        Status = SV22T1020761.Models.Sales.OrderStatusEnum.New
                    };
                    var details = new List<SV22T1020761.Models.Sales.OrderDetail>
                    {
                        new SV22T1020761.Models.Sales.OrderDetail
                        {
                            ProductID = productId,
                            Quantity = qty,
                            SalePrice = product.Price
                        }
                    };
                    int orderId = await SV22T1020761.BusinessLayers.SalesDataService.AddOrderAsync(newOrder, details);
                    System.Diagnostics.Debug.WriteLine($"Created new order: OrderID={orderId}");
                    
                    // Load the newly created cart items directly using the returned OrderID
                    var newDetails = await repo.ListDetailsAsync(orderId);
                    System.Diagnostics.Debug.WriteLine($"Order details count: {(newDetails?.Count ?? 0)}");
                    
                    var cartItems = new List<CartItem>();
                    if (newDetails != null && newDetails.Count > 0)
                    {
                        foreach (var detail in newDetails)
                        {
                            cartItems.Add(new CartItem
                            {
                                ProductID = detail.ProductID,
                                ProductName = detail.ProductName ?? "",
                                Price = detail.SalePrice,
                                Qty = detail.Quantity,
                                Photo = detail.Photo
                            });
                        }
                    }
                    var summary = (cartItems.Sum(c => c.Qty), cartItems.Sum(c => c.Price * c.Qty));
                    System.Diagnostics.Debug.WriteLine($"Returning cart summary: qty={summary.Item1}, total={summary.Item2}");
                    return PartialView("~/Views/Shared/_CartSummary.cshtml", summary);
                }
                else
                {
                    // Add item to existing draft order
                    var existing = await repo.GetDetailAsync(draftOrder.OrderID, productId);
                    System.Diagnostics.Debug.WriteLine($"Existing item: {(existing == null ? "NO" : "YES, qty=" + existing.Quantity)}");
                    
                    if (existing != null)
                    {
                        var detail = new SV22T1020761.Models.Sales.OrderDetail
                        {
                            OrderID = draftOrder.OrderID,
                            ProductID = productId,
                            Quantity = existing.Quantity + qty,
                            SalePrice = product.Price
                        };
                        await repo.UpdateDetailAsync(detail);
                    }
                    else
                    {
                        var detail = new SV22T1020761.Models.Sales.OrderDetail
                        {
                            OrderID = draftOrder.OrderID,
                            ProductID = productId,
                            Quantity = qty,
                            SalePrice = product.Price
                        };
                        await repo.AddDetailAsync(detail);
                    }
                    
                    var cart = await GetCartFromDB(customerId);
                    var summary = (cart?.Sum(c => c.Qty) ?? 0, cart?.Sum(c => c.Price * c.Qty) ?? 0);
                    System.Diagnostics.Debug.WriteLine($"Returning updated cart summary: qty={summary.Item1}, total={summary.Item2}");
                    return PartialView("~/Views/Shared/_CartSummary.cshtml", summary);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cart Add Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Cart.Index called");
            var username = User?.Identity?.Name ?? "";
            _logger.LogInformation($"Username: '{username}'");
            
            var ua = SV22T1020761.Shop.Services.AccountService.GetUser(username);
            if (ua == null || !int.TryParse(ua.UserId, out var customerId)) 
            {
                _logger.LogWarning("User not authenticated or customerId invalid");
                return Unauthorized();
            }

            _logger.LogInformation($"CustomerId: {customerId}");
            var cartItems = await GetCartFromDB(customerId);
            _logger.LogInformation($"Cart.Index returning {cartItems.Count} items");
            return View(cartItems ?? new List<CartItem>());
        }

        [HttpPost]
        public async Task<IActionResult> Update(int productId, int qty)
        {
            var username = User?.Identity?.Name ?? "";
            var ua = SV22T1020761.Shop.Services.AccountService.GetUser(username);
            if (ua == null || !int.TryParse(ua.UserId, out var customerId)) return Unauthorized();

            try
            {
                var repo = new OrderRepository(SV22T1020761.BusinessLayers.Configuration.ConnectionString);
                var orders = SV22T1020761.BusinessLayers.SalesDataService.ListOrders(
                    new SV22T1020761.Models.Sales.OrderSearchInput { Page = 1, PageSize = 100 });
                
                var draftOrder = orders?.DataItems?.FirstOrDefault(o => 
                    o.CustomerID == customerId && 
                    o.Status == SV22T1020761.Models.Sales.OrderStatusEnum.New);

                if (draftOrder != null)
                {
                    if (qty > 0)
                    {
                        var detail = new SV22T1020761.Models.Sales.OrderDetail
                        {
                            OrderID = draftOrder.OrderID,
                            ProductID = productId,
                            Quantity = qty,
                            SalePrice = 0 // Only Quantity/OrderID/ProductID matter for update
                        };
                        await repo.UpdateDetailAsync(detail);
                    }
                    else
                    {
                        await repo.DeleteDetailAsync(draftOrder.OrderID, productId);
                    }
                }
            }
            catch { }

            var cartItems = await GetCartFromDB(customerId);
            return PartialView("~/Views/Cart/_CartTable.cshtml", cartItems ?? new List<CartItem>());
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var username = User?.Identity?.Name ?? "";
            var ua = SV22T1020761.Shop.Services.AccountService.GetUser(username);
            if (ua == null || !int.TryParse(ua.UserId, out var customerId)) return Unauthorized();

            try
            {
                var repo = new OrderRepository(SV22T1020761.BusinessLayers.Configuration.ConnectionString);
                var orders = SV22T1020761.BusinessLayers.SalesDataService.ListOrders(
                    new SV22T1020761.Models.Sales.OrderSearchInput { Page = 1, PageSize = 100 });
                
                var draftOrder = orders?.DataItems?.FirstOrDefault(o => 
                    o.CustomerID == customerId && 
                    o.Status == SV22T1020761.Models.Sales.OrderStatusEnum.New);

                if (draftOrder != null)
                {
                    await repo.DeleteDetailAsync(draftOrder.OrderID, productId);
                }
            }
            catch { }

            var cartItems = await GetCartFromDB(customerId);
            return PartialView("~/Views/Cart/_CartTable.cshtml", cartItems ?? new List<CartItem>());
        }

        private async Task<List<CartItem>> GetCartFromDB(int customerId)
        {
            var cartItems = new List<CartItem>();

            try
            {
                _logger.LogInformation($"GetCartFromDB: customerId={customerId}");
                
                var repo = new OrderRepository(SV22T1020761.BusinessLayers.Configuration.ConnectionString);
                // Search for New orders (Status=1) with PageSize=1 to get only the current cart
                var orders = await Task.Run(() => 
                    SV22T1020761.BusinessLayers.SalesDataService.ListOrders(
                        new SV22T1020761.Models.Sales.OrderSearchInput 
                        { 
                            Page = 1, 
                            PageSize = 1,
                            Status = SV22T1020761.Models.Sales.OrderStatusEnum.New, // FIXED: Explicitly set Status to New (1)
                            CustomerID = customerId // Filter by customer to reduce data
                        }));
                
                _logger.LogInformation($"ListOrders returned: {orders?.DataItems?.Count ?? 0} orders");
                if (orders?.DataItems != null)
                {
                    foreach (var o in orders.DataItems)
                    {
                        _logger.LogInformation($"  - OrderID={o.OrderID}, Status={o.Status}");
                    }
                }
                
                var draftOrder = orders?.DataItems?.FirstOrDefault(o => 
                    o.Status == SV22T1020761.Models.Sales.OrderStatusEnum.New);

                _logger.LogInformation($"draftOrder found: {(draftOrder != null ? draftOrder.OrderID : "NULL")}");

                if (draftOrder != null)
                {
                    var details = await repo.ListDetailsAsync(draftOrder.OrderID);
                    _logger.LogInformation($"ListDetailsAsync returned: {details?.Count ?? 0} details");

                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            cartItems.Add(new CartItem
                            {
                                ProductID = detail.ProductID,
                                ProductName = detail.ProductName ?? "",
                                Price = detail.SalePrice,
                                Qty = detail.Quantity,
                                Photo = detail.Photo
                            });
                        }
                    }
                }
                _logger.LogInformation($"GetCartFromDB end: returning {cartItems.Count} items");
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex, $"GetCartFromDB ERROR: {ex.Message}");
            }

            return cartItems;
        }
    }
}
