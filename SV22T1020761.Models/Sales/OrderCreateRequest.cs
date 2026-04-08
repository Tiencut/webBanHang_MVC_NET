namespace SV22T1020761.Models.Sales
{
    /// <summary>
    /// Model để insert/update đơn hàng vào database
    /// Chỉ chứa các trường có thực sự trong bảng Orders
    /// </summary>
    public class OrderCreateRequest
    {
        public int? CustomerID { get; set; }
        public DateTime OrderTime { get; set; }
        public string? DeliveryProvince { get; set; }
        public string? DeliveryAddress { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? AcceptTime { get; set; }
        public int? ShipperID { get; set; }
        public DateTime? ShippedTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public OrderStatusEnum Status { get; set; }
    }
}
