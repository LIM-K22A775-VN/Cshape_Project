
using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.ViewModels;

namespace project.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly Hshop2023Context db;
        public MenuLoaiViewComponent(Hshop2023Context context)
            => db = context;
        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(
                lo => new MenuLoaiVM
                {
                    MaLoai = lo.MaLoai,
                    TenLoai = lo.TenLoai,
                    SoLuong = lo.HangHoas.Count
                }
                );
            return View(data);
        }
    }
}
