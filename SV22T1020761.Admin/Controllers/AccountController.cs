using Microsoft.AspNetCore.Mvc;

namespace SV22T1020761.Admin.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logout()
        {
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(SV22T1020761.Models.LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Simple mock authentication: accept any non-empty credentials for now
                if (!string.IsNullOrWhiteSpace(model.UserName) && !string.IsNullOrWhiteSpace(model.Password))
                {
                    TempData["SuccessMessage"] = "Đăng nhập thành công (mock).";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Đăng nhập thất bại. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogoutPost()
        {
            try
            {
                // mock logout: clear temp data and redirect to login
                TempData.Clear();
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
        public IActionResult ChangePassword(SV22T1020761.Admin.Models.ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Mock change password: accept any values and show success
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công (mock).";
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Đổi mật khẩu thất bại. Vui lòng thử lại sau.";
                return View(model);
            }
        }
    }
}
