using System.Collections.Generic;
using SV22T1020761.Models;

namespace SV22T1020761.DataLayers
{
    // Legacy DAL interfaces expected by BusinessLayers
    public interface IProductDAL
    {
        IList<Product> List(int page, int pageSize, string searchValue);
        Product? Get(int id);
        int Add(Product data);
        bool Update(Product data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    public interface ICategoryDAL
    {
        IList<Category> List(int page, int pageSize, string searchValue);
        Category? Get(int id);
        int Add(Category data);
        bool Update(Category data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    public interface ICustomerDAL
    {
        IList<Customer> List(int page, int pageSize, string searchValue);
        Customer? Get(int id);
        int Add(Customer data);
        bool Update(Customer data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    public interface IEmployeeDAL
    {
        IList<Employee> List(int page, int pageSize, string searchValue);
        Employee? Get(int id);
        int Add(Employee data);
        bool Update(Employee data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    public interface IOrderDAL
    {
        IList<Order> List(int page, int pageSize, string searchValue);
        Order? Get(int id);
        int Add(Order data);
        bool Update(Order data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    public interface IShipperDAL
    {
        IList<SV22T1020761.Models.Shipper> List(int page, int pageSize, string searchValue);
        SV22T1020761.Models.Shipper? Get(int id);
        int Add(SV22T1020761.Models.Shipper data);
        bool Update(SV22T1020761.Models.Shipper data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    public interface ISupplierDAL
    {
        IList<Supplier> List(int page, int pageSize, string searchValue);
        Supplier? Get(int id);
        int Add(Supplier data);
        bool Update(Supplier data);
        bool Delete(int id);
        int Count(string searchValue);
    }

    // Minimal stub implementations to satisfy BusinessLayers compilation
    public class ProductDB : IProductDAL
    {
        public IList<Product> List(int page, int pageSize, string searchValue) => new List<Product>();
        public Product? Get(int id) => null;
        public int Add(Product data) => 0;
        public bool Update(Product data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }

    public class CategoryDB : ICategoryDAL
    {
        public IList<Category> List(int page, int pageSize, string searchValue) => new List<Category>();
        public Category? Get(int id) => null;
        public int Add(Category data) => 0;
        public bool Update(Category data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }

    public class CustomerDB : ICustomerDAL
    {
        public IList<Customer> List(int page, int pageSize, string searchValue) => new List<Customer>();
        public Customer? Get(int id) => null;
        public int Add(Customer data) => 0;
        public bool Update(Customer data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }

    public class EmployeeDB : IEmployeeDAL
    {
        public IList<Employee> List(int page, int pageSize, string searchValue) => new List<Employee>();
        public Employee? Get(int id) => null;
        public int Add(Employee data) => 0;
        public bool Update(Employee data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }

    public class OrderDB : IOrderDAL
    {
        public IList<Order> List(int page, int pageSize, string searchValue) => new List<Order>();
        public Order? Get(int id) => null;
        public int Add(Order data) => 0;
        public bool Update(Order data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }

    public class ShipperDB : IShipperDAL
    {
        public IList<SV22T1020761.Models.Shipper> List(int page, int pageSize, string searchValue) => new List<SV22T1020761.Models.Shipper>();
        public SV22T1020761.Models.Shipper? Get(int id) => null;
        public int Add(SV22T1020761.Models.Shipper data) => 0;
        public bool Update(SV22T1020761.Models.Shipper data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }

    public class SupplierDB : ISupplierDAL
    {
        public IList<Supplier> List(int page, int pageSize, string searchValue) => new List<Supplier>();
        public Supplier? Get(int id) => null;
        public int Add(Supplier data) => 0;
        public bool Update(Supplier data) => false;
        public bool Delete(int id) => false;
        public int Count(string searchValue) => 0;
    }
}
