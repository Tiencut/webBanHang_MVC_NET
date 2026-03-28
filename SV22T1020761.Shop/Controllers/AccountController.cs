using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020761.Models.Security;
using SV22T1020761.Shop.AppCodes;
using System.Threading.Tasks;
using SV22T1020761.Shop.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

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
                ModelState.AddModelError("UserName", "Tên ðãng nh?p là b?t bu?c.");
            }
            if (string.IsNullOrWhiteSpace(model?.Email))
            {
                ModelState.AddModelError("Email", "Email là b?t bu?c.");
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("", "M?t kh?u ph?i có ít nh?t 6 k? t?.");
            }
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "M?t kh?u xác nh?n không kh?p.");
            }

            // check existing user
            var existing = AccountService.GetUser(model?.UserName);
            if (existing != null)
            {
                ModelState.AddModelError("UserName", "Tên ðãng nh?p ð? t?n t?i.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ðãng k? th?t b?i. Vui l?ng s?a các l?i r?i th? l?i.";
                return View(model);
            }

            try
            {
                await AccountService.RegisterAsync(model, password);
                TempData["Success"] = "Ðãng k? thành công. Vui l?ng ðãng nh?p.";
                return RedirectToAction("Login");
            }
            catch (System.Exception ex)
            {
                // log if you have logger
                ModelState.AddModelError("", "Có l?i khi x? l? ðãng k?. Vui l?ng th? l?i sau.");
                TempData["Error"] = "Ðãng k? th?t b?i do l?i h? th?ng.";
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
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            var user = await AccountService.ValidateUserAsync(username, password);
            if (user != null)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.NameIdentifier, user.UserId ?? "") };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                TempData["Success"] = "Ðãng nh?p thành công.";
                return RedirectToAction("Index", "Home");
            }

            // keep generic message for security
            ModelState.AddModelError("", "Tên ðãng nh?p ho?c m?t kh?u không ðúng.");
            TempData["Error"] = "Tên ðãng nh?p ho?c m?t kh?u không ðúng.";
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var user = AccountService.GetUser(User.Identity?.Name);
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
                ModelState.AddModelError("Email", "Email là b?t bu?c.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "C?p nh?t th?t b?i. Vui l?ng s?a các l?i.";
                return View(model);
            }

            AccountService.UpdateUser(model);
            TempData["Success"] = "C?p nh?t thông tin cá nhân thành công.";
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
                ModelState.AddModelError("", "M?t kh?u m?i ph?i có ít nh?t 6 k? t?.");
                TempData["Error"] = "M?t kh?u m?i ph?i có ít nh?t 6 k? t?.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Xác nh?n m?t kh?u m?i không kh?p.");
                TempData["Error"] = "Xác nh?n m?t kh?u m?i không kh?p.";
                return View();
            }

            if (username != null && AccountService.ValidatePassword(username, oldPassword))
            {
                AccountService.ChangePassword(username, newPassword);
                ViewBag.Message = "Password changed successfully.";
                TempData["Success"] = "Ð?i m?t kh?u thành công.";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "M?t kh?u c? không ðúng.");
            TempData["Error"] = "M?t kh?u c? không ðúng.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "B?n ð? ðãng xu?t.";
            return RedirectToAction("Login");
        }
    }
}