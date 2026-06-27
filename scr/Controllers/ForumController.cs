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
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem danh sách bài viết diễn đàn (Không cần đăng nhập vẫn xem được)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string category, string searchString)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Title.Contains(searchString) || p.Content.Contains(searchString));
            }

            ViewBag.CurrentCategory = category;
            ViewBag.SearchString = searchString;

            return View(await query.ToListAsync());
        }

        // Xem chi tiết bài viết và bình luận (Không cần đăng nhập vẫn xem được)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Sắp xếp bình luận cũ đến mới
            post.Comments = post.Comments.OrderBy(c => c.CreatedAt).ToList();

            return View(post);
        }

        // Đăng bài viết mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,Category")] Post post)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                post.UserId = userId;
                post.CreatedAt = DateTime.Now;

                if (ModelState.IsValid)
                {
                    _context.Add(post);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đăng bài viết mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(post);
        }

        // Bình luận bài viết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int postId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction(nameof(Details), new { id = postId });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // Xóa bài viết (Chỉ người đăng hoặc Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            if (role != "Admin" && post.UserId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa bài viết thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Xóa bình luận (Chỉ người viết hoặc Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            if (role != "Admin" && comment.UserId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            int postId = comment.PostId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa bình luận thành công!";
            return RedirectToAction(nameof(Details), new { id = postId });
        }
    }
}
