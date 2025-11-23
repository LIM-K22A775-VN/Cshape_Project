using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.ViewModels;

namespace project.Controllers
{
    public class HangHoaController : Controller
    {
		private readonly Hshop2023Context db;
		public HangHoaController(Hshop2023Context context) {
            db = context;
        }
        public IActionResult Index(int? loai)
        {
            var hangHoas  = db.HangHoas.AsQueryable();
            if (loai.HasValue)
            {
				hangHoas = hangHoas.Where(p=>p.MaLoai == loai.Value);
			}
            var result = hangHoas.Select(p=> new HangHoaVM
			{
                MaHh = p.MaHh,
                TenHH = p.TenHh,
				Dongia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                Motangan = p.MoTaDonVi ?? "",
                Tenloai = p.MaLoaiNavigation.TenLoai
            }).ToList(); ;
            return View(result);
        }
        public IActionResult Search(string? query)
        {
            var hanghoas = db.HangHoas.AsQueryable();
            if (query != null)
            {
                hanghoas = hanghoas.Where(p=>p.TenHh.Contains(query));
            }
            var result = hanghoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                Dongia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                Motangan = p.MoTaDonVi ?? "",
                Tenloai = p.MaLoaiNavigation.TenLoai
            }); 
            return View(result);
        }

        public IActionResult Detail (int id)
        {
            var data = db.HangHoas
                .Include(p=>p.MaLoaiNavigation)
                .SingleOrDefault(p=> p.MaHh == id ); 
            if(data == null)
            {
                TempData["Message"] = $"Khong tim thay san pham co ma {id}";
                return Redirect("/404");
            }

            var result = new ChiTietHangHoaVM
            {
                MaHh = data.MaHh,
                TenHH = data.TenHh,
                Dongia = data.DonGia ?? 0,
                ChiTiet = data.MoTa ?? string.Empty,
                Hinh = data.Hinh ?? string.Empty,
                Motangan = data.MoTaDonVi ?? string.Empty,
                Tenloai = data.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10,// tinh sau
                DiemDanhGia = 5, // ktra sau


            };
            return View(result);
        } 
    }
}
