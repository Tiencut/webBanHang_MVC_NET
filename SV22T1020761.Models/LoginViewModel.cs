using System.ComponentModel.DataAnnotations;

namespace SV22T1020761.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên ðãng nh?p là b?t bu?c")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
