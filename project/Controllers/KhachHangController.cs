using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.Helpers;
using project.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace project.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IMapper _mapper;

        public KhachHangController(Hshop2023Context context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(RegisterVM model) // bỏ IFormFile Hinh
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra trùng tài khoản
                    var existingUser = db.KhachHangs.FirstOrDefault(k => k.MaKh == model.MaKh);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("MaKh", "Tên đăng nhập đã tồn tại");
                        return View(model);
                    }

                    // (Tùy chọn) Kiểm tra trùng email
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var existingEmail = db.KhachHangs.FirstOrDefault(k => k.Email == model.Email);
                        if (existingEmail != null)
                        {
                            ModelState.AddModelError("Email", "Email này đã được đăng ký");
                            return View(model);
                        }
                    }

                    // Map sang KhachHang
                    var khachHang = _mapper.Map<KhachHang>(model);

                    khachHang.RandomKey = MyUtil.GenerateRandomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = true;
                    khachHang.VaiTro = 0;

                    // KHÔNG xử lý Hinh nữa – để null cũng được

                    db.Add(khachHang);
                    db.SaveChanges();

                    return RedirectToAction("Index", "HangHoa");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;  

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model,string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHangs.SingleOrDefault(
                    kh => kh.MaKh == model.UserName);
                    if (khachHang == null)
                    {
                        ModelState.AddModelError("loi", "sai thong tin dang nhap");
                    }
                    else
                    {
                    if (!khachHang.HieuLuc)
                    {
                        ModelState.AddModelError("loi", "tai khoan da bi khoa");
                    }
                    else
                    {
                        if(khachHang.MatKhau != model.Password)
                        {
                            ModelState.AddModelError("loi", "sai mat khau");
                        }
                        else
                        {
                            // Ghi nhan
                            var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email,khachHang.Email),
                                new Claim(ClaimTypes.Name,khachHang.HoTen),
                                new Claim("CustomerID",khachHang.MaKh),
                                //claim cho role
                                new Claim(ClaimTypes.Role,"Customer")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims,
                                CookieAuthenticationDefaults.AuthenticationScheme
                                );
                            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimPrincipal);
                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }

                            return RedirectToAction("Profile", "KhachHang");

                        }
                    }
                    }

                
                    
            }
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

    }
}
