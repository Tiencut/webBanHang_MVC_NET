using System.ComponentModel.DataAnnotations;

namespace SV22T1020761.Models.Partner
{
    /// <summary>
    /// Người giao hàng
    /// </summary>
    public class Shipper
    {
        /// <summary>
        /// Mã người giao hàng
        /// </summary>
        public int ShipperID { get; set; }
        /// <summary>
        /// Tên người giao hàng
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ShipperName { get; set; } = string.Empty;
        /// <summary>
        /// Điện thoại
        /// </summary>
        [Phone]
        public string? Phone { get; set; }
    }
}
