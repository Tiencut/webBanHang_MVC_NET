using System.ComponentModel.DataAnnotations;

namespace SV22T1020761.Models.Partner
{
    /// <summary>
    /// Khách hàng
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public int CustomerID { get; set; }
        /// <summary>
        /// Tên khách hàng
        /// </summary>
        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;
        /// <summary>
        /// Tên giao dịch
        /// </summary>
        [StringLength(200)]
        public string ContactName { get; set; } = string.Empty;
        /// <summary>
        /// Tỉnh/thành
        /// </summary>
        public string? Province { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Điện thoại
        /// </summary>
        [Phone]
        public string? Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Khách hàng hiện có bị khóa hay không?
        /// </summary>
        public bool? IsLocked { get; set; }
    }
}
