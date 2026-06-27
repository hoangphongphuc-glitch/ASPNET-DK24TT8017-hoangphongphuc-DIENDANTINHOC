using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Data;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Trang kiểm tra kết nối Cơ sở dữ liệu và dữ liệu người dùng
    public async Task<IActionResult> DbCheck()
    {
        var result = new System.Text.StringBuilder();
        result.AppendLine("=== HỆ THỐNG KIỂM TRA CƠ SỞ DỮ LIỆU ===");
        result.AppendLine($"Thời gian kiểm tra: {DateTime.Now}");
        result.AppendLine($"Chuỗi kết nối sử dụng: {_context.Database.GetConnectionString()}");
        
        try
        {
            // Kiểm tra khả năng kết nối
            result.AppendLine("\n1. Kiểm tra kết nối SQL Server...");
            bool canConnect = await _context.Database.CanConnectAsync();
            result.AppendLine($"   Kết quả: {(canConnect ? "THÀNH CÔNG!" : "THẤT BẠI!")}");

            if (canConnect)
            {
                // Kiểm tra các bảng hiện có
                result.AppendLine("\n2. Đang kiểm tra bảng người dùng 'DoAn'...");
                var userCount = await _context.Users.CountAsync();
                result.AppendLine($"   Số lượng bản ghi trong bảng DoAn: {userCount}");

                var sampleUsers = await _context.Users.Take(10).ToListAsync();
                result.AppendLine("\n   Danh sách 10 tài khoản đầu tiên trong bảng DoAn:");
                foreach (var u in sampleUsers)
                {
                    result.AppendLine($"   - ID: {u.Id} | Username: {u.Username} | Password: {u.Password} | Họ tên: {u.FullName} | Vai trò: {u.Role}");
                }

                // Kiểm tra các bảng khác
                result.AppendLine("\n3. Kiểm tra số lượng đề tài đồ án...");
                var projectCount = await _context.ProjectTopics.CountAsync();
                result.AppendLine($"   Số lượng đề tài: {projectCount}");

                result.AppendLine("\n4. Kiểm tra số lượng bài viết diễn đàn...");
                var postCount = await _context.Posts.CountAsync();
                result.AppendLine($"   Số lượng bài viết: {postCount}");
            }
        }
        catch (Exception ex)
        {
            result.AppendLine("\n❌ LỖI XẢY RA KHI KẾT NỐI/TRUY VẤN CSDL:");
            result.AppendLine($"   Thông báo lỗi: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.AppendLine($"   Lỗi chi tiết: {ex.InnerException.Message}");
            }
            result.AppendLine($"\n   Stack Trace:\n{ex.StackTrace}");
        }

        return Content(result.ToString(), "text/plain; charset=utf-8");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
