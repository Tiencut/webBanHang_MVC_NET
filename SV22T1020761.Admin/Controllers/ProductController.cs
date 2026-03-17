using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;

namespace SV22T1020761.Admin.Controllers
{
    public class ProductController : Controller
    {
        // =====================================================
        // Product/Index
        // =====================================================
        public IActionResult Index(string searchValue, int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var model = CatalogDataService.ListProducts(input);
            return View(model);
        }

        // =====================================================
        // Product/Create
        // =====================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                CatalogDataService.AddProduct(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // =====================================================
        // Product/Edit/{id}
        // =====================================================
        public IActionResult Edit(int id)
        {
            var product = CatalogDataService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                CatalogDataService.UpdateProduct(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // =====================================================
        // Product/Delete/{id}
        // =====================================================
        public IActionResult Delete(int id)
        {
            var product = CatalogDataService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            CatalogDataService.DeleteProduct(id);
            return RedirectToAction("Index");
        }

        // =====================================================
        // Product/ListAttributes/{id}
        // =====================================================
        public IActionResult ListAttributes(int id)
        {
            return View();
        }

        // =====================================================
        // Product/CreateAttribute/{id}
        // =====================================================
        public IActionResult CreateAttribute(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAttribute(int id, ProductAttribute model)
        {
            return View();
        }

        // =====================================================
        // Product/EditAttribute/{id}?attributeId={attributeId}
        // =====================================================
        public IActionResult EditAttribute(int id, int attributeId)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAttribute(int id, ProductAttribute model)
        {
            return View();
        }

        // =====================================================
        // Product/DeleteAttribute/{id}?attributeId={attributeId}
        // =====================================================
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            return View();
        }

        [HttpPost, ActionName("DeleteAttribute")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttributeConfirmed(int id, int attributeId)
        {
            return View();
        }

        // =====================================================
        // Product/ListPhotos/{id}
        // =====================================================
        public IActionResult ListPhotos(int id)
        {
            return View();
        }

        // =====================================================
        // Product/CreatePhoto/{id}
        // =====================================================
        public IActionResult CreatePhoto(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePhoto(int id, ProductPhoto model)
        {
            return View();
        }

        // =====================================================
        // Product/EditPhoto/{id}?photoId={photoId}
        // =====================================================
        public IActionResult EditPhoto(int id, int photoId)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPhoto(int id, ProductPhoto model)
        {
            return View();
        }

        // =====================================================
        // Product/DeletePhoto/{id}?photoId={photoId}
        // =====================================================
        public IActionResult DeletePhoto(int id, int photoId)
        {
            return View();
        }

        [HttpPost, ActionName("DeletePhoto")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePhotoConfirmed(int id, int photoId)
        {
            return View();
        }
    }
}