using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Data;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Helpers;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return View();
            }

            // Tìm người dùng theo Username (hoặc Email)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null || !PasswordHasher.VerifyPassword(password, user.Password))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View();
            }

            // Đăng nhập thành công, tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Username", user.Username)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string fullName, string email, string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            var existingUser = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
            if (existingUser)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc email đã được sử dụng.");
                return View();
            }

            // Mã hóa mật khẩu khi đăng ký tài khoản mới
            var hashedPassword = PasswordHasher.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Password = hashedPassword,
                FullName = fullName,
                Email = email,
                Role = role // SinhVien, GiangVien
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Nếu người dùng là Sinh viên, lấy thêm thông tin đồ án đang thực hiện và tiến độ
            if (user.Role == "SinhVien")
            {
                var project = await _context.ProjectTopics
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Milestones)
                    .FirstOrDefaultAsync(p => p.StudentId == user.Id);

                ViewBag.Project = project;
            }
            // Nếu người dùng là Giảng viên, lấy danh sách đề tài hướng dẫn
            else if (user.Role == "GiangVien" || user.Role == "GiaoVien")
            {
                var guidedProjects = await _context.ProjectTopics
                    .Include(p => p.Student)
                    .Where(p => p.CreatedById == user.Id)
                    .ToListAsync();

                ViewBag.GuidedProjects = guidedProjects;
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string githubUrl, string bio, string avatar)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.GithubUrl = githubUrl;
                user.Bio = bio;
                user.Avatar = avatar;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ cá nhân thành công!";
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
