using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020761.Models.Sales;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.DataLayers.SQLServer.Sales
{
    /// <summary>
    /// Repository for Orders and OrderDetails.
    /// </summary>
    public class OrderRepository : SV22T1020761.DataLayers.Interfaces.IOrderRepository
    {
        private readonly string _connectionString;
        public OrderRepository(string connectionString) => _connectionString = connectionString;

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            input ??= new OrderSearchInput();
            var result = new PagedResult<OrderViewInfo> { Page = input.Page, PageSize = input.PageSize };
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var where = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue)) { where.Add("(c.CustomerName LIKE @q OR c.ContactName LIKE @q)"); parameters.Add("q", "%" + input.SearchValue + "%"); }
            if (input.Status != OrderStatusEnum.All) { where.Add("o.Status = @Status"); parameters.Add("Status", (int)input.Status); }  // Chỉ filter nếu Status != All
            if (input.DateFrom != null) { where.Add("o.OrderTime >= @DateFrom"); parameters.Add("DateFrom", input.DateFrom); }
            if (input.DateTo != null) { where.Add("o.OrderTime <= @DateTo"); parameters.Add("DateTo", input.DateTo); }
            if (input.CustomerID != null && input.CustomerID > 0) { where.Add("o.CustomerID = @CustomerID"); parameters.Add("CustomerID", input.CustomerID); }

            string whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;

            var countSql = $"SELECT COUNT(*) FROM Orders o LEFT JOIN Customers c ON o.CustomerID = c.CustomerID {whereClause}";
            result.RowCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            string dataSql;
            if (input.PageSize == 0)
            {
                dataSql = $@"SELECT o.OrderID, o.CustomerID, c.CustomerName, c.ContactName AS CustomerContactName, c.Email AS CustomerEmail, c.Phone AS CustomerPhone, c.Address AS CustomerAddress,
o.OrderTime, o.DeliveryProvince, o.DeliveryAddress, o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime, o.Status,
ISNULL(e.FullName, '') AS EmployeeName, ISNULL(s.ShipperName, '') AS ShipperName, ISNULL(s.Phone, '') AS ShipperPhone,
(SELECT ISNULL(SUM(od.SalePrice * od.Quantity), 0) FROM OrderDetails od WHERE od.OrderID = o.OrderID) AS TotalAmount
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
{whereClause}
ORDER BY o.OrderID";
            }
            else
            {
                dataSql = $@"SELECT o.OrderID, o.CustomerID, c.CustomerName, c.ContactName AS CustomerContactName, c.Email AS CustomerEmail, c.Phone AS CustomerPhone, c.Address AS CustomerAddress,
o.OrderTime, o.DeliveryProvince, o.DeliveryAddress, o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime, o.Status,
ISNULL(e.FullName, '') AS EmployeeName, ISNULL(s.ShipperName, '') AS ShipperName, ISNULL(s.Phone, '') AS ShipperPhone,
(SELECT ISNULL(SUM(od.SalePrice * od.Quantity), 0) FROM OrderDetails od WHERE od.OrderID = o.OrderID) AS TotalAmount
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
{whereClause}
ORDER BY o.OrderID
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            var items = await conn.QueryAsync<OrderViewInfo>(dataSql, parameters);
            result.DataItems = items.AsList();
            return result;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT o.OrderID, o.CustomerID, c.CustomerName, c.ContactName AS CustomerContactName, c.Email AS CustomerEmail, c.Phone AS CustomerPhone, c.Address AS CustomerAddress,
o.OrderTime, o.DeliveryProvince, o.DeliveryAddress, o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime, o.Status,
ISNULL(e.FullName, '') AS EmployeeName, ISNULL(s.ShipperName, '') AS ShipperName, ISNULL(s.Phone, '') AS ShipperPhone,
(SELECT ISNULL(SUM(od.SalePrice * od.Quantity), 0) FROM OrderDetails od WHERE od.OrderID = o.OrderID) AS TotalAmount
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
WHERE o.OrderID = @orderID";
            return await conn.QuerySingleOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        public async Task<Order?> GetLatestAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT TOP 1 * FROM Orders ORDER BY OrderID DESC";
            return await conn.QuerySingleOrDefaultAsync<Order>(sql);
        }

        public async Task<int> AddAsync(OrderCreateRequest data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status)
VALUES (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime, @FinishedTime, @Status);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await conn.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(OrderCreateRequest data, int orderId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Orders SET CustomerID=@CustomerID, OrderTime=@OrderTime, DeliveryProvince=@DeliveryProvince, DeliveryAddress=@DeliveryAddress, EmployeeID=@EmployeeID, AcceptTime=@AcceptTime, ShipperID=@ShipperID, ShippedTime=@ShippedTime, FinishedTime=@FinishedTime, Status=@Status WHERE OrderID=@OrderID";
            var affected = await conn.ExecuteAsync(sql, new { data.CustomerID, data.OrderTime, data.DeliveryProvince, data.DeliveryAddress, data.EmployeeID, data.AcceptTime, data.ShipperID, data.ShippedTime, data.FinishedTime, data.Status, OrderID = orderId });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            // delete details first
            await conn.ExecuteAsync("DELETE FROM OrderDetails WHERE OrderID = @orderID", new { orderID });
            var affected = await conn.ExecuteAsync("DELETE FROM Orders WHERE OrderID = @orderID", new { orderID });
            return affected > 0;
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT od.OrderID, od.ProductID, od.Quantity, od.SalePrice, p.ProductName, p.Unit, p.Photo
FROM OrderDetails od
LEFT JOIN Products p ON od.ProductID = p.ProductID
WHERE od.OrderID = @orderID";
            var items = await conn.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return items.AsList();
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT od.OrderID, od.ProductID, od.Quantity, od.SalePrice, p.ProductName, p.Unit, p.Photo
FROM OrderDetails od
LEFT JOIN Products p ON od.ProductID = p.ProductID
WHERE od.OrderID = @orderID AND od.ProductID = @productID";
            return await conn.QuerySingleOrDefaultAsync<OrderDetailViewInfo>(sql, new { orderID, productID });
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice) VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "UPDATE OrderDetails SET Quantity=@Quantity, SalePrice=@SalePrice WHERE OrderID=@OrderID AND ProductID=@ProductID";
            var affected = await conn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM OrderDetails WHERE OrderID = @orderID AND ProductID = @productID";
            System.Diagnostics.Debug.WriteLine($"[SQL] DeleteDetailAsync: {sql}");
            System.Diagnostics.Debug.WriteLine($"[PARAM] orderID={orderID}, productID={productID}");
            var affected = await conn.ExecuteAsync(sql, new { orderID, productID });
            System.Diagnostics.Debug.WriteLine($"[RESULT] Rows affected: {affected}");
            return affected > 0;
        }
    }
}
