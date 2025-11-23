using System.ComponentModel.DataAnnotations;

public class RegisterVM
{
    [Display(Name = "Ten dang nhap")]
    [Required(ErrorMessage = "*")]
    [MaxLength(20, ErrorMessage = "Max 20 ky tu")]
    public string MaKh { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Mat khau")]
    [Required(ErrorMessage = "*")]
    public string MatKhau { get; set; }

    [Display(Name = "Ho ten")]
    [Required(ErrorMessage = "*")]
    [MaxLength(50, ErrorMessage = "Max 50 ky tu")]
    public string HoTen { get; set; }

    public bool GioiTinh { get; set; } = true;

    [Display(Name = "Ngay sinh")]
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "Dia chi")]
    [MaxLength(60, ErrorMessage = "Max 60 ky tu")]
    public string DiaChi { get; set; }

    [Display(Name = "Dien thoai")]
    [MaxLength(24, ErrorMessage = "Max 24 ky tu")]
    [RegularExpression(@"0[9875]\d{8}", ErrorMessage = "Chua dung dinh dang di dong viet nam")]
    public string DienThoai { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
    [Required(ErrorMessage = "*")]
    public string Email { get; set; }

    // BỎ thuộc tính này nếu không dùng
    // public string? Hinh { get; set; }
}
