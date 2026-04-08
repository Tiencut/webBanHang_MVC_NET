using System.Collections.Generic;
using SV22T1020761.Models.Common;
using SV22T1020761.Models.Sales;
using SV22T1020761.DataLayers.SQLServer.Sales;
using System.Threading.Tasks;

namespace SV22T1020761.BusinessLayers
{
    public static class SalesDataService
    {
        public static PagedResult<OrderViewInfo> ListOrders(PaginationSearchInput input)
        {
            var orderInput = new OrderSearchInput
            {
                Page = input?.Page ?? 1,
                PageSize = input?.PageSize ?? 10,
                SearchValue = input?.SearchValue ?? string.Empty,
                Status = OrderStatusEnum.All  // Không filter theo status mặc định, lấy tất cả
            };
            return ListOrders(orderInput);
        }

        public static PagedResult<OrderViewInfo> ListOrders(OrderSearchInput input)
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            var task = repo.ListAsync(input);
            task.Wait();
            return task.Result;
        }

        public static string GetCustomerName(int? customerId)
        {
            // Placeholder implementation for getting customer name
            return "Unknown Customer";
        }

        public static void AddOrder(Order order)
        {
            // Placeholder implementation for adding an order
        }

        public static Order GetOrder(int id)
        {
            // Placeholder implementation for getting an order by ID
            return new Order();
        }

        public static void UpdateOrder(Order order)
        {
            // Deprecated - use UpdateOrderAsync instead
            throw new NotImplementedException("Use UpdateOrderAsync with orderId parameter instead");
        }

        public static async Task<bool> UpdateOrderAsync(OrderCreateRequest order, int orderId)
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            return await repo.UpdateAsync(order, orderId);
        }

        public static void DeleteOrder(int id)
        {
            // Placeholder implementation for deleting an order
        }

        // New async implementations using repository
        public static async Task<int> AddOrderAsync(OrderCreateRequest order, List<OrderDetail> details)
        {
            try
            {
                Console.WriteLine("=== SalesDataService.AddOrderAsync ===");
                Console.WriteLine($"Order: CustomerID={order.CustomerID}, Province={order.DeliveryProvince}, Status={order.Status}");
                Console.WriteLine($"Details count: {details?.Count}");
                
                var repo = new OrderRepository(Configuration.ConnectionString);
                var orderId = await repo.AddAsync(order);
                Console.WriteLine($"Inserted order: ID={orderId}");
                
                if (details != null && details.Count > 0)
                {
                    foreach (var d in details)
                    {
                        d.OrderID = orderId;
                        await repo.AddDetailAsync(d);
                        Console.WriteLine($"Added detail: ProductID={d.ProductID}, Qty={d.Quantity}, Price={d.SalePrice}");
                    }
                }
                
                Console.WriteLine($"Order creation completed successfully: ID={orderId}");
                return orderId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in AddOrderAsync: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public static async Task<OrderViewInfo?> GetOrderAsync(int id)
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            return await repo.GetAsync(id);
        }

        public static async Task<List<OrderDetailViewInfo>> ListOrderDetailsAsync(int orderId)
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            return await repo.ListDetailsAsync(orderId);
        }

        public static Order GetLatestOrder()
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            var task = repo.GetLatestAsync();
            task.Wait();
            return task.Result ?? new Order();
        }

        public static void AddOrderDetail(OrderDetail detail)
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            var task = repo.AddDetailAsync(detail);
            task.Wait();
        }
    }
}