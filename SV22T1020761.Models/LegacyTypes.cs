// Adapter types to provide root namespace types expected by BusinessLayers
using System;
using SV22T1020761.Models.Catalog;
using SV22T1020761.Models.Partner;
using SV22T1020761.Models.HR;
using SV22T1020761.Models.Sales;

namespace SV22T1020761.Models
{
    // Thin wrappers that inherit existing detailed models to expose them under SV22T1020761.Models
    public class Product : Catalog.Product { }
    public class Category : Catalog.Category { }
    public class ProductAttribute : Catalog.ProductAttribute { }
    public class ProductPhoto : Catalog.ProductPhoto { }

    public class Customer : Partner.Customer { }
    public class Shipper : Partner.Shipper { }
    public class Supplier : Partner.Supplier { }

    public class Employee : HR.Employee { }

    public class Order : Sales.Order { }
}
