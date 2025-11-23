using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.ViewModels;
using project.Helpers;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Authorization;
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

        public IActionResult Checkout()
        {
            var cart = Cart;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            // Gửi danh sách sản phẩm sang view Checkout
            return View(cart);
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

            // Gửi danh sách sản phẩm sang view ThanhToan
            return View(cart);
        }


        [Authorize]
        [HttpPost]
        public IActionResult ThanhToan(string HoTen, string DienThoai, string DiaChi,
                                      string Email, string CachThanhToan)
        {
            var cart = Cart;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            // Lấy MaKH từ tài khoản đăng nhập (Email)
            string? maKh = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Ở đây giả sử User.Identity.Name = Email khách hàng
                var kh = db.KhachHangs.SingleOrDefault(k => k.Email == User.Identity.Name);
                if (kh != null)
                {
                    maKh = kh.MaKh;   // chú ý: thuộc tính trong model phải là MaKh (mapping MaKH trong DB)
                }
            }

            // Tạo hóa đơn (mapping với bảng HoaDon trong DB)
            var hoaDon = new HoaDon
            {
                MaKh = maKh,              // nullable
                NgayDat = DateTime.Now,
                NgayCan = DateTime.Now,      // tuỳ bạn dùng
                NgayGiao = DateTime.Now.AddDays(3),
                HoTen = HoTen,
                DiaChi = DiaChi,
                CachThanhToan = CachThanhToan,
                CachVanChuyen = "Ship COD",
                PhiVanChuyen = 0,
                MaTrangThai = 1,
                GhiChu = $"ĐT: {DienThoai}, Email: {Email}"
            };

            db.HoaDons.Add(hoaDon);
            db.SaveChanges();   // sinh MaHd

            foreach (var item in cart)
            {
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

            db.SaveChanges();

            HttpContext.Session.Remove(CART_KEY);

            TempData["Message"] = $"Đặt hàng thành công. Mã hóa đơn: {hoaDon.MaHd}";
            return RedirectToAction("Index", "Home");
        }


    }
}
