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
        public async Task<IActionResult> Register(UserAccount model, string password)
        {
            if (ModelState.IsValid)
            {
                await AccountService.RegisterAsync(model, password);
                return RedirectToAction("Login");
            }
            return View(model);
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
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password.");
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
            if (ModelState.IsValid)
            {
                AccountService.UpdateUser(model);
                ViewBag.Message = "Profile updated successfully.";
            }
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
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            var username = User.Identity?.Name;
            if (username != null && AccountService.ValidatePassword(username, oldPassword))
            {
                AccountService.ChangePassword(username, newPassword);
                ViewBag.Message = "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Old password is incorrect.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}