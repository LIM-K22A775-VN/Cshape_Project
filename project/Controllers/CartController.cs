using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.ViewModels;
using project.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace project.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;

        public CartController(Hshop2023Context context) {
           db  = context;
        }
        string CART_KEY = MySetting.CART_KEY;
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ??
            new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }

        // Hàm tạo GhiChu từ Email và DienThoai
        private string? BuildGhiChu(string? email, string? dienThoai)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(email))
            {
                parts.Add($"Email: {email}");
            }
            if (!string.IsNullOrEmpty(dienThoai))
            {
                parts.Add($"ĐT: {dienThoai}");
            }
            if (parts.Any())
            {
                var ghiChu = string.Join(", ", parts);
                return ghiChu.Length > 50 ? ghiChu.Substring(0, 47) + "..." : ghiChu;
            }
            return null;
        }
        

        public IActionResult AddToCart(int id,int quantity = 1)
        {
            var giohang = Cart;
            var item = giohang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hanghoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hanghoa == null) {
                    TempData["Message"] = $"Khong tim thay hang hoa";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hanghoa.MaHh,
                    TenHh = hanghoa.TenHh,
                    DonGia = hanghoa.DonGia ?? 0,
                    Hinh = hanghoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                giohang.Add(item);

            }
            else
            {
                item.SoLuong += quantity;
            }
            HttpContext.Session.Set(CART_KEY, giohang);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id) {
            var giohang = Cart;
            var item = giohang.SingleOrDefault(p=>p.MaHh == id);
            if (item != null) {
                giohang.Remove(item);
                HttpContext.Session.Set(CART_KEY,giohang);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            try
            {
                var giohang = Cart;
                var item = giohang.SingleOrDefault(p => p.MaHh == id);
                if (item != null && quantity > 0)
                {
                    item.SoLuong = quantity;
                    HttpContext.Session.Set(CART_KEY, giohang);
                    
                    var subtotal = giohang.Sum(p => p.ThanhTien);
                    var totalQuantity = giohang.Sum(p => p.SoLuong);
                    
                    return Json(new { 
                        success = true, 
                        subtotal = subtotal,
                        totalQuantity = totalQuantity
                    });
                }
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc số lượng không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Checkout()
        {
            var cart = Cart;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            // Nếu chưa đăng nhập, yêu cầu đăng nhập
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["Message"] = "Vui lòng đăng nhập để tiếp tục thanh toán";
                return RedirectToAction("DangNhap", "KhachHang", new { ReturnUrl = "/Cart/ThanhToan" });
            }

            // Redirect đến trang thanh toán
            return RedirectToAction("ThanhToan");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ThanhToan()
        {
            var cart = Cart;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            // Lấy thông tin khách hàng đã đăng nhập để tự động điền form
            CheckoutVM model = new CheckoutVM();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var kh = db.KhachHangs.SingleOrDefault(k => k.Email == email);
                    if (kh != null)
                    {
                        model.HoTen = kh.HoTen ?? string.Empty;
                        model.DienThoai = kh.DienThoai ?? string.Empty;
                        model.DiaChi = kh.DiaChi ?? string.Empty;
                        model.Email = kh.Email;
                    }
                }
            }

            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(p => p.ThanhTien);
            return View(model);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThanhToan(CheckoutVM model)
        {
            var cart = Cart;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                ViewBag.Total = cart.Sum(p => p.ThanhTien);
                return View(model);
            }

            // Lấy MaKH từ tài khoản đăng nhập
            string maKh = string.Empty;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Lấy CustomerID từ claim
                var customerId = User.FindFirst("CustomerID")?.Value;
                if (!string.IsNullOrEmpty(customerId))
                {
                    maKh = customerId;
                }
                else
                {
                    // Fallback: tìm theo Email
                    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        var kh = db.KhachHangs.SingleOrDefault(k => k.Email == email);
                        if (kh != null)
                        {
                            maKh = kh.MaKh;
                        }
                    }
                }
            }

            // Nếu không tìm thấy MaKh, yêu cầu đăng nhập lại
            if (string.IsNullOrEmpty(maKh))
            {
                TempData["Message"] = "Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại.";
                return RedirectToAction("DangNhap", "KhachHang", new { ReturnUrl = "/Cart/ThanhToan" });
            }

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra khách hàng có tồn tại không
                    var khachHang = db.KhachHangs.FirstOrDefault(k => k.MaKh == maKh);
                    if (khachHang == null)
                    {
                        throw new Exception($"Không tìm thấy khách hàng với mã: {maKh}");
                    }

                    // Kiểm tra trạng thái có tồn tại không, nếu không có thì lấy trạng thái đầu tiên
                    var trangThai = db.TrangThais.FirstOrDefault(t => t.MaTrangThai == 1);
                    int maTrangThai = 1;
                    if (trangThai == null)
                    {
                        // Nếu không có trạng thái 1, lấy trạng thái đầu tiên có sẵn
                        var firstTrangThai = db.TrangThais.FirstOrDefault();
                        if (firstTrangThai == null)
                        {
                            throw new Exception("Không tìm thấy trạng thái đơn hàng trong hệ thống");
                        }
                        maTrangThai = firstTrangThai.MaTrangThai;
                    }

                    // Kiểm tra dữ liệu bắt buộc
                    if (string.IsNullOrWhiteSpace(model.DiaChi))
                    {
                        throw new Exception("Địa chỉ không được để trống");
                    }
                    if (string.IsNullOrWhiteSpace(model.CachThanhToan))
                    {
                        throw new Exception("Cách thanh toán không được để trống");
                    }

                    // Tính tổng tiền
                    var tongTien = cart.Sum(p => p.ThanhTien);
                    var phiVanChuyen = 30000; // Phí vận chuyển 30.000 đ

                    // Tạo hóa đơn
                    var hoaDon = new HoaDon
                    {
                        MaKh = maKh,
                        NgayDat = DateTime.Now,
                        NgayCan = DateTime.Now,
                        NgayGiao = DateTime.Now.AddDays(3),
                        HoTen = model.HoTen ?? string.Empty,
                        DiaChi = model.DiaChi.Trim(),
                        // DienThoai không có trong database, lưu vào GhiChu thay thế
                        CachThanhToan = model.CachThanhToan.Trim(),
                        CachVanChuyen = "Ship COD",
                        PhiVanChuyen = phiVanChuyen,
                        MaTrangThai = maTrangThai, // Trạng thái đơn hàng
                        GhiChu = BuildGhiChu(model.Email, model.DienThoai)
                    };

                    db.HoaDons.Add(hoaDon);
                    db.SaveChanges();   // Lưu để sinh MaHd

                    // Thêm chi tiết hóa đơn cho từng sản phẩm
                    foreach (var item in cart)
                    {
                        // Kiểm tra hàng hóa có tồn tại không
                        var hangHoa = db.HangHoas.FirstOrDefault(h => h.MaHh == item.MaHh);
                        if (hangHoa == null)
                        {
                            throw new Exception($"Không tìm thấy hàng hóa với mã: {item.MaHh}");
                        }

                        // Kiểm tra số lượng hợp lệ
                        if (item.SoLuong <= 0)
                        {
                            throw new Exception($"Số lượng sản phẩm '{item.TenHh}' không hợp lệ");
                        }

                        var ct = new ChiTietHd
                        {
                            MaHd = hoaDon.MaHd,
                            MaHh = item.MaHh,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            GiamGia = 0
                        };
                        db.ChiTietHds.Add(ct);
                    }

                    // Lưu tất cả chi tiết hóa đơn
                    db.SaveChanges();

                    // Commit transaction - lưu tất cả vào database
                    transaction.Commit();

                    // Xóa giỏ hàng sau khi thanh toán thành công
                    HttpContext.Session.Remove(CART_KEY);

                    TempData["Message"] = $"Đặt hàng thành công! Mã hóa đơn: {hoaDon.MaHd}";
                    TempData["Success"] = true;
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    try
                    {
                        transaction.Rollback();
                    }
                    catch { }

                    // Hiển thị lỗi chi tiết hơn
                    var errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " | Chi tiết: " + ex.InnerException.Message;
                    }

                    ModelState.AddModelError("", $"Có lỗi xảy ra khi xử lý đơn hàng: {errorMessage}");
                    ViewBag.Cart = cart;
                    ViewBag.Total = cart.Sum(p => p.ThanhTien);
                    return View(model);
                }
            }
        }


    }
}
