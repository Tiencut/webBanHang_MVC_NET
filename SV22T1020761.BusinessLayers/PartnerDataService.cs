using System.Threading.Tasks;
using SV22T1020761.DataLayers.Interfaces;
using SV22T1020761.DataLayers.SQLServer.Partner;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Partner;

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// lớp cung cấp các chức năng tác nghiệp
    /// đến các đối tác của hẹ thống,bao gồm:Supplier,shipper,Customer
    /// </summary>
    public class PartnerDataService
    {
        private static readonly IGenericRepository<Supplier> supplierDB;
        private static readonly IGenericRepository<Shipper> shipperDB;
        private static readonly ICustomerRepository customerDB;
        ///<summary>
        ///Constructor
        ///</summary>
        static PartnerDataService()
        {
            supplierDB = new SupplierRepository(Configuration.ConnectionString);
            shipperDB = new ShipperRepository(Configuration.ConnectionString);
            customerDB = new CustomerRepository(Configuration.ConnectionString);
        }
        //== các chức năng liên quan đến nhà cung cấp
        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await supplierDB.ListAsync(input);
        }
        /// <summary>
        /// bổ sung một nhà cung cấp
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns>
        /// hàm trả về mã nhà cung cấp được bổ sung
        /// </returns>
        public static async Task<Supplier?> GetSupplierAsync(int supplierID)
        {
            /// todo: kiểm tra tính hợp lệ  của dữ liệu trước khi bổ sung
            return await supplierDB.GetAsync(supplierID);
        }
        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            //TODO: kiểm tra tính hợp lệ của dữ liệu trước khi cập nhật
            return await supplierDB.UpdateAsync(supplier);
        }
        public static async Task<bool> DeleteSupploerAsync(int supplierID)
        {
            if (await supplierDB.IsUsedAsync(supplierID))
                return false;
            return await supplierDB.DeleteAsync(supplierID);

        }
        /// <summary>
        /// kiểm tra xem một nhà cung cấp có mặt hàng kiên quan hay không
        /// để biết là có thể xoá được không
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public static async Task<bool> IsUsedSupplierAsync(int supplierID)
        {
            return await supplierDB.IsUsedAsync(supplierID);
        }

        // == Shipper
        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
        {
            return await shipperDB.ListAsync(input);
        }

        public static async Task<Shipper?> GetShipperAsync(int shipperID)
        {
            return await shipperDB.GetAsync(shipperID);
        }

        public static async Task<int> AddShipperAsync(Shipper data)
        {
            return await shipperDB.AddAsync(data);
        }

        public static async Task<bool> UpdateShipperAsync(Shipper data)
        {
            return await shipperDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            return await shipperDB.DeleteAsync(shipperID);
        }

        // == Customer
        public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
        {
            return await customerDB.ListAsync(input);
        }

        public static async Task<Customer?> GetCustomerAsync(int customerID)
        {
            return await customerDB.GetAsync(customerID);
        }

        public static async Task<int> AddCustomerAsync(Customer data)
        {
            return await customerDB.AddAsync(data);
        }

        public static async Task<bool> UpdateCustomerAsync(Customer data)
        {
            return await customerDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            return await customerDB.DeleteAsync(customerID);
        }

        /// <summary>
        /// Check mail của khách hợp lệ?
        /// Hợp lệ nếu không trùng
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<bool> ValidateCustomerEmailAsync(string email, int id = 0)
        {
            return await customerDB.ValidateEmailAsync(email, id);
        }
    }
}