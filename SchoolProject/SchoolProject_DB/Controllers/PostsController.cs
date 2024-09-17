using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;

namespace SchoolProject_DB.Controllers
{
    public class PostsController : Controller
    {
        private readonly SchoolProjectContext _context;

        public PostsController(SchoolProjectContext context)
        {
            _context = context;
        }


        // GET: Posts
        public async Task<IActionResult> Index()
        {
            // 從 Session 中取得 MemberID，並放入 ViewBag
            var memberID = HttpContext.Session.GetString("MemberID");
            ViewBag.SessionMemberID = memberID;

            // 取得按 UpdatedAt 或 CreatedAt 排序的文章
            var posts = await _context.Post
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post
                .Include(p => p.Member)
                .FirstOrDefaultAsync(m => m.PostID == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public async Task<IActionResult> Create()
        {
            // 找到當前資料庫中最大的 PostID
            var maxPostID = await _context.Post
                                          .OrderByDescending(p => p.PostID)
                                          .Select(p => p.PostID)
                                          .FirstOrDefaultAsync();

            // 如果資料庫是空的，預設 PostID 從 00000001 開始
            string newPostID = "00000001";
            if (!string.IsNullOrEmpty(maxPostID))
            {
                // 轉換為數字，加 1 後轉回 8 位數字的字串
                newPostID = (int.Parse(maxPostID) + 1).ToString("D8");
            }

            // 將新的 PostID 存入 Session
            HttpContext.Session.SetString("NewPostID", newPostID);

            // 將 newPostID 傳遞到視圖
            ViewBag.NewPostID = newPostID;

            // 確保當前使用者的 MemberID 已經在 Session 中
            var loggedInMemberID = HttpContext.Session.GetString("MemberID");
            if (string.IsNullOrEmpty(loggedInMemberID))
            {
                // 如果沒有 MemberID，重定向到登入頁面
                return RedirectToAction("Login", "Account");
            }
            ViewBag.NewMemberID = loggedInMemberID;

            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostID,MemberID,PostTitle,Description,CreatedAt,UpdatedAt,Photos,ImageType")] Post post, IFormFile? photo)
        {
            // 從 Session 中取得 PostID 和 MemberID
            if (string.IsNullOrEmpty(post.PostID))
            {
                post.PostID = HttpContext.Session.GetString("NewPostID");
                if (string.IsNullOrEmpty(post.PostID))
                {
                    ModelState.AddModelError("", "PostID 未生成，請重新嘗試。");
                    return View(post);
                }
            }

            var loggedInMemberID = HttpContext.Session.GetString("MemberID");
            if (string.IsNullOrEmpty(loggedInMemberID))
            {
                return RedirectToAction("Login", "Account");
            }

            post.MemberID = loggedInMemberID;

            // 處理圖片上傳
            if (photo != null && (photo.ContentType == "image/jpeg" || photo.ContentType == "image/png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await photo.CopyToAsync(memoryStream);
                    post.Photos = memoryStream.ToArray();
                    post.ImageType = photo.ContentType;
                }
            }

            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now; // 設定當前時間
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // 成功後回到文章列表頁面
            }

            return View(post); // 返回原始表單並顯示錯誤訊息
        }




        // 不准編輯文章
        

        // GET: Posts/Delete/5 
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post
                .Include(p => p.Member)
                .FirstOrDefaultAsync(m => m.PostID == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var post = await _context.Post.FindAsync(id);
            if (post != null)
            {
                _context.Post.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(string id)
        {
            return _context.Post.Any(e => e.PostID == id);
        }

    }
}
