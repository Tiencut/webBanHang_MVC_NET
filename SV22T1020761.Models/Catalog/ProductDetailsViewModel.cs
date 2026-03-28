using System.Collections.Generic;

namespace SV22T1020761.Models.Catalog
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = new Product();
        public List<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
        public List<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }
    }
}
