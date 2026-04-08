using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.HR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using SV22T1020761.Admin.AppCodes;

namespace SV22T1020761.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ILogger<EmployeeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            try
            {
                var pagedResult = HRDataService.ListEmployees(input);
                return View(pagedResult);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading employees");
                TempData["Error"] = "Không thể kết nối tới CSDL. Vui lòng kiểm tra cấu hình và thử lại.";
                var empty = new PagedResult<Employee> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<Employee>() };
                return View(empty);
            }
        }

        // Return partial form for modal (create or edit)
        [HttpGet]
        public async Task<IActionResult> Form(int? id, bool delete = false)
        {
            if (id == null || id == 0)
            {
                ViewBag.Title = "Bổ sung nhân viên";
                var model = new Employee() { EmployeeID = 0, IsWorking = true };
                if (delete) return BadRequest();
                return PartialView("_EmployeeForm", model);
            }
            else
            {
                ViewBag.Title = "Cập nhật thông tin nhân viên";
                var model = await HRDataService.GetEmployeeAsync(id.Value);
                if (model == null) return NotFound();
                if (delete) return PartialView("_EmployeeDelete", model);
                return PartialView("_EmployeeForm", model);
            }
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân vien";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân vien";

                //Ki?m tra dữ liệu �?u v�o: FullName v� Email l� b?t bu?c, Email ch�a ��?c s? d?ng b?i nhân viên kh�c
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return PartialView("_EmployeeForm", data);
                    return View("Edit", data);
                }

                //X? l? upload ?nh
                if (uploadPhoto != null)
                {
                    var fileName = $"{System.Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "employees");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                //Ti?n x? l? dữ liệu tr�?c khi Lưu v�o database
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                //Lưu dữ liệu v�o database (Bổ sung ho?c Cập nhật)
                if (data.EmployeeID == 0)
                {
                    await HRDataService.AddEmployeeAsync(data);
                }
                else
                {
                    await HRDataService.UpdateEmployeeAsync(data);
                }

                // If AJAX, return updated table partial
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = HRDataService.ListEmployees(input);
                    return PartialView("_EmployeeTable", result);
                }

                TempData["Success"] = data.EmployeeID == 0 ? "Bổ sung nhân viên thành công." : "Cập nhật nhân viên thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                // Log the error and return the Edit view with a friendly message
                _logger?.LogError(ex, "Error saving employee data (EmployeeID={EmployeeID}).", data?.EmployeeID);
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_EmployeeForm", data);
                return View("Edit", data);
            }
        }

        public IActionResult Delete(int id)
        {
            var employee = HRDataService.GetEmployee(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                HRDataService.DeleteEmployee(id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = HRDataService.ListEmployees(input);
                    return PartialView("_EmployeeTable", result);
                }

                TempData["Success"] = "Xóa nhân viên thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting employee (EmployeeID={EmployeeID}).", id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest("Lỗi khi xóa: " + ex.Message);
                }
                // Try to reload the employee to show details and the error message
                var employee = HRDataService.GetEmployee(id);
                ModelState.AddModelError(string.Empty, "Không thể xóa nhân viên. Vui lòng kiểm tra dữ liệu hoặc thử lại sau.");
                return View("Delete", employee);
            }
        }

        [HttpPost]
        public IActionResult Search(PaginationSearchInput input)
        {
            // Save search conditions to session
            ApplicationContext.SetSessionData("EmployeeSearchConditions", input);

            // Fetch data based on search conditions
            var result = HRDataService.ListEmployees(input);

            // Return partial view with updated data
            return PartialView("_EmployeeTable", result);
        }
    }
}
