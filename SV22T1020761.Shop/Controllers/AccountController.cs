using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models.Security;
using SV22T1020761.Shop.AppCodes;
using System.Threading.Tasks;
using SV22T1020761.Shop.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;

namespace SV22T1020761.Shop.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserAccount model, string password, string confirmPassword)
        {
            // basic server-side validation
            if (string.IsNullOrWhiteSpace(model?.UserName))
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(model?.Email))
            {
                ModelState.AddModelError("Email", "Email là bắt buộc.");
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 kí tự.");
            }
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
            }

            // check existing user
            var existing = AccountService.GetUser(model?.UserName);
            if (existing != null)
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Đăng ký thất bại. Vui lòng sửa các lỗi rồi thử lại.";
                return View(model);
            }

            try
            {
                await AccountService.RegisterAsync(model, password);
                TempData["Success"] = "Đăng ký thành công! Dữ liệu lưu vào Database. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe, string returnUrl = null)
        {
            var user = await AccountService.ValidateUserAsync(username, password);
            if (user != null)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.NameIdentifier, user.UserId ?? "") };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                
                // Check if there's a pending product in session
                var pendingProductId = HttpContext.Session.GetInt32("PendingProductId");
                var pendingQty = HttpContext.Session.GetInt32("PendingProductQty") ?? 1;
                
                if (pendingProductId.HasValue && pendingProductId.Value > 0)
                {
                    try
                    {
                        // Auto-add the pending product to cart
                        var product = SV22T1020761.BusinessLayers.CatalogDataService.GetProduct(pendingProductId.Value);
                        if (product != null && int.TryParse(user.UserId, out var customerId))
                        {
                            var repo = new SV22T1020761.DataLayers.SQLServer.Sales.OrderRepository(SV22T1020761.BusinessLayers.Configuration.ConnectionString);
                            
                            // Get or create draft order
                            var orders = SV22T1020761.BusinessLayers.SalesDataService.ListOrders(
                                new SV22T1020761.Models.Sales.OrderSearchInput 
                                { 
                                    Page = 1, 
                                    PageSize = 100,
                                    Status = SV22T1020761.Models.Sales.OrderStatusEnum.New,
                                    CustomerID = customerId 
                                });
                            
                            var draftOrder = orders?.DataItems?.FirstOrDefault(o => 
                                o.Status == SV22T1020761.Models.Sales.OrderStatusEnum.New);
                            
                            if (draftOrder == null)
                            {
                                // Create new order
                                var newOrder = new SV22T1020761.Models.Sales.Order
                                {
                                    CustomerID = customerId,
                                    OrderTime = System.DateTime.Now,
                                    Status = SV22T1020761.Models.Sales.OrderStatusEnum.New
                                };
                                var details = new System.Collections.Generic.List<SV22T1020761.Models.Sales.OrderDetail>
                                {
                                    new SV22T1020761.Models.Sales.OrderDetail
                                    {
                                        ProductID = pendingProductId.Value,
                                        Quantity = pendingQty,
                                        SalePrice = product.Price
                                    }
                                };
                                await SV22T1020761.BusinessLayers.SalesDataService.AddOrderAsync(newOrder, details);
                            }
                            else
                            {
                                // Add to existing order
                                var existing = await repo.GetDetailAsync(draftOrder.OrderID, pendingProductId.Value);
                                if (existing != null)
                                {
                                    existing.Quantity += pendingQty;
                                    await repo.UpdateDetailAsync(existing);
                                }
                                else
                                {
                                    var detail = new SV22T1020761.Models.Sales.OrderDetail
                                    {
                                        OrderID = draftOrder.OrderID,
                                        ProductID = pendingProductId.Value,
                                        Quantity = pendingQty,
                                        SalePrice = product.Price
                                    };
                                    await repo.AddDetailAsync(detail);
                                }
                            }
                        }
                        
                        // Clear session
                        HttpContext.Session.Remove("PendingProductId");
                        HttpContext.Session.Remove("PendingProductQty");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error auto-adding product: {ex.Message}");
                    }
                }
                
                TempData["Success"] = "đăng nhập thành công.";
                
                // Redirect to returnUrl if provided, else Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            // keep generic message for security
            ModelState.AddModelError("", "Tên đăng nhập hoặc Mật khẩu không đúng.");
            TempData["Error"] = "Tên đăng nhập hoặc Mật khẩu không đúng.";
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var user = AccountService.GetUser(User.Identity?.Name);
            
            // Load provinces for dropdown
            var provinces = DictionaryDataService.ListProvinces(new PaginationSearchInput 
            { 
                Page = 1, 
                PageSize = 1000
            });
            
            ViewBag.Provinces = provinces.DataItems ?? new List<string>();
            
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(UserAccount model)
        {
            // ensure username not tampered
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser)) return Forbid();

            model.UserName = currentUser; // enforce

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email là bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Cập nhật thất bại. Vui lòng sửa các lỗi.";
                
                // Reload provinces
                var provinces = DictionaryDataService.ListProvinces(new PaginationSearchInput 
                { 
                    Page = 1, 
                    PageSize = 1000
                });
                ViewBag.Provinces = provinces.DataItems ?? new List<string>();
                
                return View(model);
            }

            // Update user
            AccountService.UpdateUser(model);
            
            // Reload updated user data
            model = AccountService.GetUser(currentUser);
            
            // Reload provinces
            var provincesAfterUpdate = DictionaryDataService.ListProvinces(new PaginationSearchInput 
            { 
                Page = 1, 
                PageSize = 1000
            });
            ViewBag.Provinces = provincesAfterUpdate.DataItems ?? new List<string>();
            
            TempData["Success"] = "Cập nhật thông tin cá nhân thành công.";
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải có ít nhất 6 kí tự.");
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 kí tự.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Xác nhận Mật khẩu mới không khớp.");
                TempData["Error"] = "Xác nhận Mật khẩu mới không khớp.";
                return View();
            }

            if (username != null && AccountService.ValidatePassword(username, oldPassword))
            {
                AccountService.ChangePassword(username, newPassword);
                ViewBag.Message = "Password changed successfully.";
                TempData["Success"] = "Đổi Mật khẩu thành công.";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Mật khẩu cũ không đúng.");
            TempData["Error"] = "Mật khẩu cũ không đúng.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Login");
        }
    }
}