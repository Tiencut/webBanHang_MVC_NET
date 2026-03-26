using System.ComponentModel.DataAnnotations;

namespace SV22T1020761.Models.Partner
{
    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }
        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;
        /// <summary>
        /// Tên giao dịch
        /// </summary>
        [StringLength(200)]
        public string ContactName { get; set; } = string.Empty;
        /// <summary>
        /// Tỉnh thành
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
        public string? Email { get; set; }
    }
}
