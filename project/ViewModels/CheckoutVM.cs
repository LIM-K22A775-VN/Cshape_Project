using System.ComponentModel.DataAnnotations;

namespace project.ViewModels
{
    public class CheckoutVM
    {
        [Display(Name = "Họ tên người nhận")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string DienThoai { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [MaxLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string DiaChi { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? Email { get; set; }

        [Display(Name = "Cách thanh toán")]
        [Required(ErrorMessage = "Vui lòng chọn cách thanh toán")]
        public string CachThanhToan { get; set; } = "Tiền mặt";
    }
}
