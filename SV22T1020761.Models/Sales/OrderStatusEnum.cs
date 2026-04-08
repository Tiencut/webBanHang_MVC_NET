namespace SV22T1020761.Models.Sales
{
    /// <summary>
    /// Định nghĩa các trạng thái của đơn hàng
    /// </summary>
    public enum OrderStatusEnum
    {
        /// <summary>
        /// Không filter theo trạng thái (lấy tất cả)
        /// </summary>
        All = -99,
        /// <summary>
        /// Đơn hàng bị từ chối
        /// </summary>
        Rejected = -2,
        /// <summary>
        /// Đơn hàng bị hủy
        /// </summary>
        Cancelled = -1,
        /// <summary>
        /// Đơn hàng nháp (giỏ hàng tạm)
        /// </summary>
        Draft = 0,
        /// <summary>
        /// Đơn hàng vừa được tạo, chưa được xử lý
        /// </summary>
        New = 1,
        /// <summary>
        /// Đơn hàng đã được duyệt chấp nhận
        /// </summary>
        Accepted = 2,
        /// <summary>
        /// Đơn hàng đang được giao cho người giao hàng để vận chuyển đến khách hàng
        /// </summary>
        Shipping = 3,
        /// <summary>
        /// Đơn hàng đã hoàn tất (thành công)
        /// </summary>
        Completed = 4
    }
}
