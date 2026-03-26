using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;
using Microsoft.Extensions.Logging;

namespace SV22T1020761.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

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

            try
            {
                var model = CatalogDataService.ListProducts(input);
                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading products");
                TempData["Error"] = "Không thể kết nối tới cơ sở dữ liệu. Vui lòng kiểm tra cấu hình và thử lại.";
                var empty = new PagedResult<Product> { Page = page, PageSize = pageSize, RowCount = 0, DataItems = new System.Collections.Generic.List<Product>() };
                return View(empty);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(PaginationSearchInput input)
        {
            var result = CatalogDataService.ListProducts(input);
            return PartialView("_ProductTable", result);
        }

        // Partial form for modal
        [HttpGet]
        public IActionResult Form(int? id, bool delete = false)
        {
            if (id == null || id == 0)
            {
                var model = new Product();
                if (delete) return BadRequest();
                return PartialView("_ProductForm", model);
            }
            var product = CatalogDataService.GetProduct(id.Value);
            if (product == null) return NotFound();
            if (delete) return PartialView("_ProductDelete", product);
            return PartialView("_ProductForm", product);
        }

        // =====================================================
        // Product/Create
        // =====================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            try
            {
                if (!ModelState.IsValid) return View(product);
                CatalogDataService.AddProduct(product);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = CatalogDataService.ListProducts(input);
                    return PartialView("_ProductTable", result);
                }

                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating product");
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(product);
            }
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            try
            {
                if (!ModelState.IsValid) return View(product);
                CatalogDataService.UpdateProduct(product);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = CatalogDataService.ListProducts(input);
                    return PartialView("_ProductTable", result);
                }

                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating product (Id={ProductId})", product?.ProductID);
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                return View(product);
            }
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
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                CatalogDataService.DeleteProduct(id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var input = new PaginationSearchInput { Page = 1, PageSize = 10, SearchValue = "" };
                    var result = CatalogDataService.ListProducts(input);
                    return PartialView("_ProductTable", result);
                }

                TempData["Success"] = "Xóa sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting product (Id={ProductId})", id);
                var product = CatalogDataService.GetProduct(id);
                ModelState.AddModelError(string.Empty, "Không thể xóa sản phẩm. Vui lòng thử lại sau.");
                return View("Delete", product);
            }
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