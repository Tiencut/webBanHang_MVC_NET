using Microsoft.AspNetCore.Mvc;
using SV22T1020761.BusinessLayers;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Catalog;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SV22T1020761.Admin.Controllers
{
    [Authorize]
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
        [HttpGet]
        public IActionResult Index(string searchValue = "", int categoryId = 0, int supplierId = 0, decimal minPrice = 0, decimal maxPrice = 0, int page = 1, int pageSize = 10)
        {
            var input = new ProductSearchInput
            {
                SearchValue = searchValue,
                CategoryID = categoryId,
                SupplierID = supplierId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
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
        public IActionResult Search(ProductSearchInput input)
        {
            var result = CatalogDataService.ListProducts(input);
            return PartialView("_ProductTable", result);
        }

        // Partial form for modal
        [HttpGet]
        public IActionResult Form(int? id, bool delete = false)
        {
            try
            {
                var model = new ProductDetailsViewModel();
                
                if (id == null || id == 0)
                {
                    model.Product = new Product();
                    if (delete) return BadRequest();
                    var categoryInput = new PaginationSearchInput { PageSize = 0 };
                    var supplierInput = new PaginationSearchInput { PageSize = 0 };
                    ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
                    ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
                    return PartialView("_ProductForm", model);
                }
                
                var product = CatalogDataService.GetProduct(id.Value);
                if (product == null) return NotFound();
                if (delete) return PartialView("_ProductDelete", product);
                
                model.Product = product;
                model.Attributes = new System.Collections.Generic.List<ProductAttribute>();
                model.Photos = new System.Collections.Generic.List<ProductPhoto>();
                
                var categoryInput2 = new PaginationSearchInput { PageSize = 0 };
                var supplierInput2 = new PaginationSearchInput { PageSize = 0 };
                ViewBag.Categories = CatalogDataService.ListCategories(categoryInput2).DataItems;
                ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput2).DataItems;
                return PartialView("_ProductForm", model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading form (Id={ProductId}, Delete={Delete})", id, delete);
                TempData["Error"] = "Hệ thống đang bận. Vui lòng thử lại sau.";
                return BadRequest();
            }
        }

        // =====================================================
        // Product/Create
        // =====================================================
        public IActionResult Create()
        {
            var categoryInput = new PaginationSearchInput { PageSize = 0 };
            var supplierInput = new PaginationSearchInput { PageSize = 0 };
            ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
            ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductDetailsViewModel model)
        {
            var isAjax = Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
            
            try
            {
                if (!ModelState.IsValid)
                {
                    var categoryInput = new PaginationSearchInput { PageSize = 0 };
                    var supplierInput = new PaginationSearchInput { PageSize = 0 };
                    ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
                    ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
                    
                    if (isAjax)
                        return PartialView("_ProductForm", model);
                    return View(model);
                }

                var product = model.Product;
                if (string.IsNullOrEmpty(product.Photo)) product.Photo = "nophoto.png";

                CatalogDataService.AddProduct(product);

                if (isAjax)
                    return Json(new { success = true, redirectUrl = Url.Action("Index") });
                
                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating product");
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                var categoryInput = new PaginationSearchInput { PageSize = 0 };
                var supplierInput = new PaginationSearchInput { PageSize = 0 };
                ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
                ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
                
                if (isAjax)
                    return PartialView("_ProductForm", model);
                return View(model);
            }
        }

        // =====================================================
        // Product/Edit/{id}
        // =====================================================
        public IActionResult Edit(int id)
        {
            try
            {
                var product = CatalogDataService.GetProduct(id);
                if (product == null)
                {
                    return NotFound();
                }
                var attributes = CatalogDataService.ListAttributes(id);
                var photos = CatalogDataService.ListProductPhotos(id);
                var model = new ProductDetailsViewModel
                {
                    Product = product,
                    Attributes = attributes,
                    Photos = photos
                };
                // Load categories and suppliers for dropdowns
                var categoryInput = new PaginationSearchInput { PageSize = 0 };
                var supplierInput = new PaginationSearchInput { PageSize = 0 };
                ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
                ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
                ViewBag.Photos = photos;
                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading product for edit (Id={ProductId})", id);
                TempData["Error"] = "Không thể tải sản phẩm. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductDetailsViewModel model)
        {
            var isAjax = Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
            
            try
            {
                Console.WriteLine($"=== EDIT PRODUCT POST ===");
                Console.WriteLine($"ProductID: {model?.Product?.ProductID}");
                Console.WriteLine($"ProductName: {model?.Product?.ProductName}");
                Console.WriteLine($"Price: {model?.Product?.Price}");
                Console.WriteLine($"CategoryID: {model?.Product?.CategoryID}");
                Console.WriteLine($"SupplierID: {model?.Product?.SupplierID}");
                Console.WriteLine($"Quantity: {model?.Product?.Quantity}");
                Console.WriteLine($"Photo: {model?.Product?.Photo}");
                Console.WriteLine($"IsSelling: {model?.Product?.IsSelling}");
                Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid) 
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    Console.WriteLine($"❌ Validation Errors:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  - {error.ErrorMessage}");
                    }
                    var categoryInput = new PaginationSearchInput { PageSize = 0 };
                    var supplierInput = new PaginationSearchInput { PageSize = 0 };
                    ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
                    ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
                    
                    if (isAjax)
                        return PartialView("_ProductForm", model);
                    return View(model);
                }

                var product = model.Product;
                
                // Nếu không chọn ảnh từ dropdown, giữ ảnh cũ từ database
                if (string.IsNullOrEmpty(product.Photo))
                {
                    var oldProduct = CatalogDataService.GetProduct(product.ProductID);
                    if (oldProduct != null && !string.IsNullOrEmpty(oldProduct.Photo))
                    {
                        product.Photo = oldProduct.Photo;
                    }
                    else
                    {
                        product.Photo = "nophoto.png";
                    }
                }

                Console.WriteLine($"✏️ Before Update - Product: {product.ProductName}, CategoryID: {product.CategoryID}, SupplierID: {product.SupplierID}");
                CatalogDataService.UpdateProduct(product);
                Console.WriteLine($"✏️ Update Complete");

                if (isAjax)
                    return Json(new { success = true, redirectUrl = Url.Action("Index") });
                
                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating product (Id={ProductId})", model?.Product?.ProductID);
                Console.WriteLine($"❌ EXCEPTION: {ex.Message}");
                Console.WriteLine($"❌ STACK: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận. Vui lòng thử lại sau.");
                var categoryInput = new PaginationSearchInput { PageSize = 0 };
                var supplierInput = new PaginationSearchInput { PageSize = 0 };
                ViewBag.Categories = CatalogDataService.ListCategories(categoryInput).DataItems;
                ViewBag.Suppliers = PartnerDataService.ListSuppliers(supplierInput).DataItems;
                
                if (isAjax)
                    return PartialView("_ProductForm", model);
                return View(model);
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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest("Lỗi khi xóa: " + ex.Message);
                }
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
            try
            {
                var product = CatalogDataService.GetProduct(id);
                if (product == null) return NotFound();
                var attributes = CatalogDataService.ListAttributes(id);
                var model = new ProductDetailsViewModel
                {
                    Product = product,
                    Attributes = attributes,
                    Photos = new System.Collections.Generic.List<ProductPhoto>()
                };
                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading product attributes (Id={ProductId})", id);
                TempData["Error"] = "Không thể tải thuộc tính sản phẩm. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        // =====================================================
        // Product/CreateAttribute/{id}
        // =====================================================
        public IActionResult CreateAttribute(int id)
        {
            try
            {
                var product = CatalogDataService.GetProduct(id);
                if (product == null) return NotFound();
                ViewBag.ProductID = id;
                ViewBag.ProductName = product.ProductName;
                return View();
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading create attribute form (Id={ProductId})", id);
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAttribute(int id, ProductAttribute model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ProductID = id;
                    return View(model);
                }

                var product = CatalogDataService.GetProduct(id);
                if (product == null) return NotFound();

                model.ProductID = id;
                CatalogDataService.AddProductAttribute(model);

                TempData["Success"] = "Thêm thuộc tính sản phẩm thành công.";
                return Redirect($"{Url.Action("Edit", new { id })}#attributes");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating product attribute (Id={ProductId})", id);
                ModelState.AddModelError(string.Empty, "Lỗi khi thêm thuộc tính. Vui lòng thử lại sau.");
                ViewBag.ProductID = id;
                return View(model);
            }
        }

        // =====================================================
        // Product/EditAttribute/{id}?attributeId={attributeId}
        // =====================================================
        public IActionResult EditAttribute(int id, int attributeId)
        {
            try
            {
                var attribute = CatalogDataService.GetProductAttribute(attributeId);
                if (attribute == null || attribute.ProductID != id) return NotFound();
                ViewBag.ProductID = id;
                return View(attribute);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading edit attribute form (Id={ProductId}, AttributeId={AttributeId})", id, attributeId);
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAttribute(int id, int attributeId, ProductAttribute model)
        {
            try
            {
                Console.WriteLine($"✏️ EDIT ATTRIBUTE - ID: {id}, AttributeId: {attributeId}");
                Console.WriteLine($"✏️ Model Binding - AttributeID: {model.AttributeID}, ProductID: {model.ProductID}");
                Console.WriteLine($"✏️ Model Values - Name: '{model.AttributeName}', Value: '{model.AttributeValue}', Order: {model.DisplayOrder}");
                Console.WriteLine($"✏️ ModelState Valid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"❌ Validation Error: {error.ErrorMessage}");
                    }
                    ViewBag.ProductID = id;
                    return View(model);
                }

                var attribute = CatalogDataService.GetProductAttribute(attributeId);
                if (attribute == null || attribute.ProductID != id) return NotFound();

                model.ProductID = id;
                model.AttributeID = attributeId;
                
                Console.WriteLine($"✏️ Before Update - Setting AttributeID: {model.AttributeID}");
                CatalogDataService.UpdateProductAttribute(model);
                Console.WriteLine($"✏️ Update Complete");

                TempData["Success"] = "Cập nhật thuộc tính sản phẩm thành công.";
                return Redirect($"{Url.Action("Edit", new { id })}#attributes");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating product attribute (Id={ProductId}, AttributeId={AttributeId})", id, attributeId);
                Console.WriteLine($"❌ EXCEPTION: {ex.Message}");
                Console.WriteLine($"❌ STACK: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật thuộc tính. Vui lòng thử lại sau.");
                ViewBag.ProductID = id;
                return View(model);
            }
        }

        // =====================================================
        // Product/DeleteAttribute/{id}?attributeId={attributeId}
        // =====================================================
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            try
            {
                var attribute = CatalogDataService.GetProductAttribute(attributeId);
                if (attribute == null || attribute.ProductID != id) return NotFound();
                ViewBag.ProductID = id;
                return View(attribute);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading delete attribute form (Id={ProductId}, AttributeId={AttributeId})", id, attributeId);
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("DeleteAttribute")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttributeConfirmed(int id, int attributeId)
        {
            try
            {
                var attribute = CatalogDataService.GetProductAttribute(attributeId);
                if (attribute == null || attribute.ProductID != id) return NotFound();

                CatalogDataService.DeleteProductAttribute(attributeId);

                TempData["Success"] = "Xóa thuộc tính sản phẩm thành công.";
                return Redirect($"{Url.Action("Edit", new { id })}#attributes");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting product attribute (Id={ProductId}, AttributeId={AttributeId})", id, attributeId);
                TempData["Error"] = "Lỗi khi xóa thuộc tính. Vui lòng thử lại sau.";
                return Redirect($"{Url.Action("Edit", new { id })}#attributes");
            }
        }

        // =====================================================
        // Product/ListPhotos/{id}
        // =====================================================
        public IActionResult ListPhotos(int id)
        {
            try
            {
                var product = CatalogDataService.GetProduct(id);
                if (product == null) return NotFound();
                var photos = CatalogDataService.ListProductPhotos(id);
                var model = new ProductDetailsViewModel
                {
                    Product = product,
                    Photos = photos,
                    Attributes = new System.Collections.Generic.List<ProductAttribute>()
                };
                return View(model);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading product photos (Id={ProductId})", id);
                TempData["Error"] = "Không thể tải ảnh sản phẩm. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        // =====================================================
        // Product/CreatePhoto/{id}
        // =====================================================
        public IActionResult CreatePhoto(int id)
        {
            try
            {
                var product = CatalogDataService.GetProduct(id);
                if (product == null) return NotFound();
                ViewBag.ProductID = id;
                ViewBag.ProductName = product.ProductName;
                return View();
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading create photo form (Id={ProductId})", id);
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePhoto(int id, IFormFile photo, string description = "")
        {
            try
            {
                if (photo == null || photo.Length == 0)
                {
                    ModelState.AddModelError("photo", "Vui lòng chọn một tệp ảnh.");
                    ViewBag.ProductID = id;
                    return View();
                }

                var product = CatalogDataService.GetProduct(id);
                if (product == null) return NotFound();

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(photo.ContentType))
                {
                    ModelState.AddModelError("photo", "Chỉ chấp nhận các tệp ảnh (jpg, png, gif, webp).");
                    ViewBag.ProductID = id;
                    return View();
                }

                // Save file
                var uploadsDir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(photo.FileName);
                var filePath = System.IO.Path.Combine(uploadsDir, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    photo.CopyTo(stream);
                }

                // Save to database
                var productPhoto = new ProductPhoto
                {
                    ProductID = id,
                    Photo = fileName,
                    DisplayOrder = 0,
                    Description = description
                };

                CatalogDataService.AddProductPhoto(productPhoto);

                TempData["Success"] = "Thêm ảnh sản phẩm thành công.";
                return Redirect($"{Url.Action("Edit", new { id })}#photos");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error creating product photo (Id={ProductId})", id);
                ModelState.AddModelError(string.Empty, "Lỗi khi thêm ảnh. Vui lòng thử lại sau.");
                ViewBag.ProductID = id;
                return View();
            }
        }

        // =====================================================
        // Product/EditPhoto/{id}?photoId={photoId}
        // =====================================================
        public IActionResult EditPhoto(int id, long photoId)
        {
            try
            {
                var photo = CatalogDataService.GetProductPhoto(photoId);
                if (photo == null || photo.ProductID != id) return NotFound();
                ViewBag.ProductID = id;
                return View(photo);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading edit photo form (Id={ProductId}, PhotoId={PhotoId})", id, photoId);
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPhoto(int id, long photoId, ProductPhoto model)
        {
            try
            {
                var photo = CatalogDataService.GetProductPhoto(photoId);
                if (photo == null || photo.ProductID != id) return NotFound();

                photo.Description = model.Description;
                photo.DisplayOrder = model.DisplayOrder;

                CatalogDataService.UpdateProductPhoto(photo);

                TempData["Success"] = "Cập nhật ảnh sản phẩm thành công.";
                return Redirect($"{Url.Action("Edit", new { id })}#photos");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error updating product photo (Id={ProductId}, PhotoId={PhotoId})", id, photoId);
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật ảnh. Vui lòng thử lại sau.");
                ViewBag.ProductID = id;
                return View(model);
            }
        }

        // =====================================================
        // Product/DeletePhoto/{id}?photoId={photoId}
        // =====================================================
        public IActionResult DeletePhoto(int id, long photoId)
        {
            try
            {
                var photo = CatalogDataService.GetProductPhoto(photoId);
                if (photo == null || photo.ProductID != id) return NotFound();
                ViewBag.ProductID = id;
                return View(photo);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error loading delete photo form (Id={ProductId}, PhotoId={PhotoId})", id, photoId);
                TempData["Error"] = "Lỗi khi tải form. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("DeletePhoto")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePhotoConfirmed(int id, long photoId)
        {
            try
            {
                var photo = CatalogDataService.GetProductPhoto(photoId);
                if (photo == null || photo.ProductID != id) return NotFound();

                // Delete file from disk
                var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", photo.Photo);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Delete from database
                CatalogDataService.DeleteProductPhoto(photoId);

                TempData["Success"] = "Xóa ảnh sản phẩm thành công.";
                return Redirect($"{Url.Action("Edit", new { id })}#photos");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error deleting product photo (Id={ProductId}, PhotoId={PhotoId})", id, photoId);
                TempData["Error"] = "Lỗi khi xóa ảnh. Vui lòng thử lại sau.";
                return Redirect($"{Url.Action("Edit", new { id })}#photos");
            }
        }
    }
}