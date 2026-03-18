namespace SV22T1020761.Models.Sales
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public int? CustomerID { get; set; }
        /// <summary>
        /// Thời điểm đặt hàng (thời điểm tạo đơn hàng)
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Tỉnh/thành giao hàng
        /// </summary>
        public string? DeliveryProvince { get; set; }
        /// <summary>
        /// Địa chỉ giao hàng
        /// </summary>
        public string? DeliveryAddress { get; set; }
        /// <summary>
        /// Mã nhân viên xử lý đơn hàng (người nhận/duyệt đơn hàng)
        /// </summary>
        public int? EmployeeID { get; set; }
        /// <summary>
        /// Thời điểm duyệt đơn hàng (thời điểm nhân viên nhận/duyệt đơn hàng)
        /// </summary>
        public DateTime? AcceptTime { get; set; }
        /// <summary>
        /// Mã người giao hàng
        /// </summary>
        public int? ShipperID { get; set; }
        /// <summary>
        /// Thời điểm người giao hàng nhận đơn hàng để giao
        /// </summary>
        public DateTime? ShippedTime { get; set; }
        /// <summary>
        /// Thời điểm kết thúc đơn hàng
        /// </summary>
        public DateTime? FinishedTime { get; set; }
        /// <summary>
        /// Trạng thái hiện tại của đơn hàng
        /// </summary>
        public OrderStatusEnum Status { get; set; }
        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string? CustomerName { get; set; }
        /// <summary>
        /// Thời điểm đặt hàng (thời điểm tạo đơn hàng)
        /// </summary>
        public DateTime? OrderDate { get; set; }
        /// <summary>
        /// Tổng tiền đơn hàng
        /// </summary>
        public decimal? TotalAmount { get; set; }
    }

    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public int? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public DateTime OrderTime { get; set; }
        public string? DeliveryProvince { get; set; }
        public string? DeliveryAddress { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? AcceptTime { get; set; }
        public int? ShipperID { get; set; }
        public DateTime? ShippedTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public OrderStatusEnum Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
