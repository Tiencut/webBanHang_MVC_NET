using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;

namespace SV22T1020761.Admin.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var model = CatalogDataService.ListCategories(input);
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                CatalogDataService.AddCategory(category);
                return RedirectToAction("Index");
            }
            return View(category);
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
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                CatalogDataService.UpdateCategory(category);
                return RedirectToAction("Index");
            }
            return View(category);
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
        public IActionResult DeleteConfirmed(int id)
        {
            CatalogDataService.DeleteCategory(id);
            return RedirectToAction("Index");
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
