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
            var repo = new OrderRepository(Configuration.ConnectionString);
            var orderInput = new OrderSearchInput
            {
                Page = input?.Page ?? 1,
                PageSize = input?.PageSize ?? 10,
                SearchValue = input?.SearchValue ?? string.Empty,
                // leave other filters to defaults
            };
            // call repository synchronously (repo has async method)
            var task = repo.ListAsync(orderInput);
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
            // Placeholder implementation for updating an order
        }

        public static void DeleteOrder(int id)
        {
            // Placeholder implementation for deleting an order
        }

        // New async implementations using repository
        public static async Task<int> AddOrderAsync(Order order, List<OrderDetail> details)
        {
            var repo = new OrderRepository(Configuration.ConnectionString);
            var orderId = await repo.AddAsync(order);
            if (details != null)
            {
                foreach (var d in details)
                {
                    d.OrderID = orderId;
                    await repo.AddDetailAsync(d);
                }
            }
            return orderId;
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
    }
}