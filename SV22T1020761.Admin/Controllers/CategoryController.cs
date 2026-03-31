using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;
using Microsoft.Extensions.Logging;

namespace SV22T1020761.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ILogger<CategoryController> logger)
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
                var model = CatalogDataService.ListCategories(input);
                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading categories");
                TempData["Error"] = "Không thể kết nối tới cơ sở dữ liệu. Vui lòng kiểm tra cấu hình và thử lại.";
                var empty = new PagedResult<Category> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<Category>() };
                return View(empty);
            }
        }

        [HttpGet]
        public IActionResult Form(int? id, bool delete = false)
        {
            if (id == null || id == 0)
            {
                var model = new Category();
                if (delete) return BadRequest();
                return PartialView("_CategoryForm", model);
            }
            var category = CatalogDataService.GetCategory(id.Value);
            if (category == null) return NotFound();
            if (delete) return PartialView("_CategoryDelete", category);
            return PartialView("_CategoryForm", category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            try
            {
                if (!ModelState.IsValid) return View(category);
                CatalogDataService.AddCategory(category);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = CatalogDataService.ListCategories(input);
                    return PartialView("_CategoryTable", result);
                }

                TempData["Success"] = "Thêm danh mục thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating category");
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(category);
            }
        }

        public IActionResult Edit(int id)
        {
            var category = CatalogDataService.GetCategory(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            try
            {
                if (!ModelState.IsValid) return View(category);
                CatalogDataService.UpdateCategory(category);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = CatalogDataService.ListCategories(input);
                    return PartialView("_CategoryTable", result);
                }

                TempData["Success"] = "Cập nhật danh mục thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating category (Id={CategoryId})", category?.CategoryID);
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(category);
            }
        }

        public IActionResult Delete(int id)
        {
            var category = CatalogDataService.GetCategory(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                CatalogDataService.DeleteCategory(id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = CatalogDataService.ListCategories(input);
                    return PartialView("_CategoryTable", result);
                }
                TempData["Success"] = "Xóa danh mục thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting category (Id={CategoryId})", id);
                var category = CatalogDataService.GetCategory(id);
                ModelState.AddModelError(string.Empty, "Không thể xóa danh mục. Vui lòng thử lại sau.");
                return View("Delete", category);
            }
        }

        [HttpPost]
        public IActionResult Search(PaginationSearchInput input)
        {
            ApplicationContext.SetSessionData("CategorySearchConditions", input);
            var result = CatalogDataService.ListCategories(input);
            return PartialView("_CategoryTable", result);
        }
    }
}
