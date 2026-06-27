using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Data;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem danh sách đề tài đồ án
        [AllowAnonymous]
        public async Task<IActionResult> Index(string category, string statusSearch)
        {
            var query = _context.ProjectTopics
                .Include(p => p.CreatedBy)
                .Include(p => p.Student)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(statusSearch))
            {
                query = query.Where(p => p.Status == statusSearch);
            }

            ViewBag.CurrentCategory = category;
            ViewBag.CurrentStatus = statusSearch;

            return View(await query.ToListAsync());
        }

        // Xem chi tiết đề tài đồ án & tiến độ
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.ProjectTopics
                .Include(p => p.CreatedBy)
                .Include(p => p.Student)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // Giảng viên thêm đề tài đồ án mới
        [HttpGet]
        public IActionResult Create()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "GiangVien" && role != "GiaoVien" && role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Category")] ProjectTopic topic)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "GiangVien" && role != "GiaoVien" && role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                topic.CreatedById = userId;
                topic.Status = "Chờ đăng ký";

                if (ModelState.IsValid)
                {
                    _context.Add(topic);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm đề tài đồ án thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(topic);
        }

        // Sinh viên đăng ký đề tài đồ án
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "SinhVien")
            {
                TempData["ErrorMessage"] = "Chỉ sinh viên mới có quyền đăng ký đề tài.";
                return RedirectToAction(nameof(Index));
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int studentId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem sinh viên đã có đồ án nào khác chưa
            var hasProject = await _context.ProjectTopics.AnyAsync(p => p.StudentId == studentId);
            if (hasProject)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký hoặc đang thực hiện một đề tài đồ án khác rồi.";
                return RedirectToAction(nameof(Index));
            }

            var topic = await _context.ProjectTopics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            if (topic.Status != "Chờ đăng ký")
            {
                TempData["ErrorMessage"] = "Đề tài này không còn ở trạng thái chờ đăng ký.";
                return RedirectToAction(nameof(Index));
            }

            topic.StudentId = studentId;
            topic.Status = "Chờ duyệt";

            _context.Update(topic);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký đề tài thành công! Vui lòng chờ Giảng viên phê duyệt.";
            return RedirectToAction(nameof(Index));
        }

        // Giảng viên phê duyệt đăng ký đồ án của sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var project = await _context.ProjectTopics
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            // Kiểm tra quyền (chỉ giảng viên hướng dẫn của đề tài đó hoặc Admin mới được duyệt)
            if (role != "Admin" && project.CreatedById != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (project.Status != "Chờ duyệt")
            {
                TempData["ErrorMessage"] = "Đồ án này không ở trạng thái chờ duyệt đăng ký.";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }

            project.Status = "Đang làm";
            _context.Update(project);

            // Tự động sinh ra 3 mốc tiến độ cụ thể cho sinh viên
            var now = DateTime.Now;
            var milestones = new Milestone[]
            {
                new Milestone
                {
                    ProjectId = project.Id,
                    Title = "Mốc 1: Báo cáo đề cương chi tiết",
                    Description = "Sinh viên chuẩn bị file báo cáo đề cương, đăng tải lên GitHub cá nhân và nộp link tại đây để giáo viên phê duyệt.",
                    DueDate = now.AddDays(7),
                    Status = "Chưa nộp"
                },
                new Milestone
                {
                    ProjectId = project.Id,
                    Title = "Mốc 2: Báo cáo kết quả giữa kỳ",
                    Description = "Sinh viên hoàn thành 50% khối lượng công việc, viết báo cáo giữa kỳ và nộp link GitHub chứa mã nguồn ứng dụng.",
                    DueDate = now.AddDays(21),
                    Status = "Chưa nộp"
                },
                new Milestone
                {
                    ProjectId = project.Id,
                    Title = "Mốc 3: Báo cáo nghiệm thu & sản phẩm hoàn thiện",
                    Description = "Sinh viên hoàn thiện toàn bộ mã nguồn chương trình, viết báo cáo nghiệm thu tổng kết, nộp sản phẩm kèm link video demo nếu có.",
                    DueDate = now.AddDays(35),
                    Status = "Chưa nộp"
                }
            };

            foreach (var m in milestones)
            {
                _context.Milestones.Add(m);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã phê duyệt đề tài và khởi tạo 3 mốc tiến độ đồ án thành công!";
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }

        // Sinh viên nộp bài báo cáo tiến độ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitMilestone(int milestoneId, string githubSubmitUrl)
        {
            var milestone = await _context.Milestones
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            // Chỉ sinh viên được giao đồ án đó mới được nộp
            if (milestone.Project?.StudentId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            milestone.GithubSubmitUrl = githubSubmitUrl;
            milestone.Status = "Chờ duyệt";
            milestone.SubmittedAt = DateTime.Now;

            _context.Update(milestone);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã nộp link GitHub tiến độ thành công! Vui lòng chờ Giảng viên đánh giá.";
            return RedirectToAction("Profile", "Account");
        }

        // Giảng viên đánh giá mốc tiến độ (Đạt / Không đạt)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluateMilestone(int id, string status, string feedback)
        {
            var milestone = await _context.Milestones
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (milestone == null)
            {
                return NotFound();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            if (role != "Admin" && milestone.Project?.CreatedById != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            milestone.Status = status; // Đạt, Không đạt
            milestone.Feedback = feedback;

            _context.Update(milestone);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật đánh giá mốc tiến độ thành: {status}";
            return RedirectToAction(nameof(Details), new { id = milestone.ProjectId });
        }

        // Giảng viên chấm điểm đồ án kết thúc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeProject(int id, double grade, string feedback)
        {
            var project = await _context.ProjectTopics.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            if (role != "Admin" && project.CreatedById != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            project.Grade = grade;
            project.Feedback = feedback;
            project.Status = "Hoàn thành";

            _context.Update(project);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã chấm điểm tổng kết và hoàn thành đồ án thành công!";
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }

        // Admin xóa đề tài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var topic = await _context.ProjectTopics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            _context.ProjectTopics.Remove(topic);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa đề tài đồ án thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
