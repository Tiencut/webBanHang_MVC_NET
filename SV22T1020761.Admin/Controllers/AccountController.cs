using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Admin.AppCodes;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models;
using SV22T1020761.Models.HR;

namespace SV22T1020761.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải trang đăng nhập. Vui lòng thử lại sau.";
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword()
        {
            try
            {
                return View();
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return View();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(SV22T1020761.Models.LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Kiểm tra tài khoản thật từ DB (bảng Employee)
                var email = model.UserName?.Trim();
                var password = model.Password;
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                    return View(model);
                }

                // Mã hóa mật khẩu
                var hashedPassword = CryptHelper.HashMD5(password);
                var employee = await HRDataService.GetEmployeeByEmailAsync(email);
                if (employee == null || string.Compare(employee.Password, hashedPassword, true) != 0)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Tạo WebUserData từ thông tin nhân viên thật
                var userData = new WebUserData
                {
                    UserId = employee.EmployeeID.ToString(),
                    UserName = employee.Email,
                    DisplayName = employee.FullName,
                    Email = employee.Email,
                    Photo = employee.Photo ?? "default.png",
                    Roles = string.IsNullOrEmpty(employee.RoleNames)
                        ? new List<string>()
                        : employee.RoleNames.Split(',').Select(r => r.Trim()).ToList()
                };

                var principal = userData.CreatePrincipal();
                await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                });

                TempData["Success"] = $"Xin chào {employee.FullName}! Đăng nhập thành công.";
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Đăng nhập thất bại. Vui lòng thử lại sau: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            try
            {
                await HttpContext.SignOutAsync("Cookies");
                return RedirectToAction("Login");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Đăng xuất thất bại. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SV22T1020761.Admin.Models.ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var currentUser = User.GetUserData();
                if (currentUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Không thể xác định người dùng hiện tại.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.OldPassword))
                {
                    ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu cũ không được để trống.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới không được để trống.");
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "Xác nhận mật khẩu không khớp.");
                    return View(model);
                }

                // Lấy thông tin nhân viên hiện tại từ DB
                if (!int.TryParse(currentUser.UserId, out var employeeId))
                {
                    ModelState.AddModelError(string.Empty, "Không thể xác định mã nhân viên.");
                    return View(model);
                }

                var employee = await HRDataService.GetEmployeeAsync(employeeId);
                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin nhân viên.");
                    return View(model);
                }

                // Kiểm tra mật khẩu cũ
                var oldHashedPassword = CryptHelper.HashMD5(model.OldPassword);
                if (string.Compare(employee.Password, oldHashedPassword, true) != 0)
                {
                    ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu cũ không đúng.");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                employee.Password = CryptHelper.HashMD5(model.NewPassword);
                await HRDataService.UpdateEmployeeAsync(employee);

                TempData["Success"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Đổi mật khẩu thất bại. Vui lòng thử lại sau: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                    TempData["Error"] = $"Dữ liệu không hợp lệ: {errors}";
                    return RedirectToAction("Login");
                }

                // Check if username/email already exists
                var existingEmployee = await HRDataService.GetEmployeeByEmailAsync(model.Email);
                if (existingEmployee != null)
                {
                    TempData["Error"] = "Email này đã được đăng ký.";
                    return RedirectToAction("Login");
                }

                if (string.IsNullOrWhiteSpace(model.UserName))
                {
                    TempData["Error"] = "Tên đăng nhập không được để trống.";
                    return RedirectToAction("Login");
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    TempData["Error"] = "Email không được để trống.";
                    return RedirectToAction("Login");
                }

                if (model.Password != model.ConfirmPassword)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp.";
                    return RedirectToAction("Login");
                }

                // Create new employee from registration data
                var newEmployee = new SV22T1020761.Models.HR.Employee
                {
                    FullName = model.DisplayName,
                    Email = model.Email,
                    Password = CryptHelper.HashMD5(model.Password),
                    Photo = "default.png",
                    IsWorking = true,
                    RoleNames = "User" // Default role
                };

                // Save to database
                var employeeId = await HRDataService.AddEmployeeAsync(newEmployee);

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Đăng ký thất bại. Vui lòng thử lại sau: " + ex.Message;
                return RedirectToAction("Login");
            }
        }
    }
}
