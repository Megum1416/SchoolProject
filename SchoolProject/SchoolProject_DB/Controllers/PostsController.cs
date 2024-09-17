using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;
using SchoolProject_DB.ViewModels;

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
            var postsWithRePostCount = await _context.Post
                                   .Include(p => p.Member) // 包含 Member 資料
                                   .Select(post => new PostWithRePostCountViewModel
                                   {
                                       Post = post,
                                       RePostCount = _context.RePost.Count(r => r.PostID == post.PostID) // 計算留言數
                                   })
                                   .ToListAsync();

            // 傳遞 session 的 MemberID 和 IsAdmin
            var sessionMemberID = HttpContext.Session.GetString("MemberID");
            ViewBag.SessionMemberID = sessionMemberID;

            var loggedInMember = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == sessionMemberID);
            ViewBag.IsAdmin = loggedInMember?.IsAdmin; // 設置 IsAdmin 狀態

            return View(postsWithRePostCount);
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

            // 傳遞 session 的 MemberID 和 IsAdmin
            var sessionMemberID = HttpContext.Session.GetString("MemberID");
            ViewBag.SessionMemberID = sessionMemberID;

            var loggedInMember = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == sessionMemberID);
            ViewBag.IsAdmin = loggedInMember?.IsAdmin; // 設置 IsAdmin 狀態

            return View(post);
        }

        public async Task<IActionResult> GetImage(string id)
        {
            var post = await _context.Post.FindAsync(id); // 根據 PostID 查詢文章

            if (post == null || post.Photos == null) // 確保文章存在且有圖片
            {
                return NotFound(); // 返回404 Not Found
            }

            return File(post.Photos, post.ImageType); // 返回文章的圖片
        }


        // GET: Posts/Create
        public async Task<IActionResult> Create()
        {
            // 找到當前資料庫中最大的 PostID
            var maxPostID = await _context.Post
                                          .OrderByDescending(p => p.PostID)
                                          .Select(p => p.PostID)
                                          .FirstOrDefaultAsync();

            // 設定新的 PostID，若資料庫是空的，預設為 00000001
            string newPostID = string.IsNullOrEmpty(maxPostID) ? "00000001" : (int.Parse(maxPostID) + 1).ToString("D8");

            // 將新的 PostID 存入 Session
            HttpContext.Session.SetString("NewPostID", newPostID);
            ViewBag.NewPostID = newPostID;

            // 確保當前使用者的 MemberID 已經在 Session 中
            var loggedInMemberID = HttpContext.Session.GetString("MemberID");
            if (string.IsNullOrEmpty(loggedInMemberID))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.NewMemberID = loggedInMemberID;

            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile? uploadPhoto)
        {
            // 設定 PostID 從 Session 獲取
            post.PostID = HttpContext.Session.GetString("NewPostID");

            // 檢查 PostID 和 MemberID 是否存在
            if (string.IsNullOrEmpty(post.PostID) || string.IsNullOrEmpty(post.MemberID))
            {
                ModelState.AddModelError("", "PostID 或 MemberID 未生成，請重新嘗試。");
                return View(post);
            }

            //上傳照片的處理
            if (uploadPhoto != null)
            {
                if (uploadPhoto.ContentType != "image/jpeg" && uploadPhoto.ContentType != "image/png")
                {
                    ViewData["Message"] = "僅能上傳JPG或PNG檔。";
                    return View(post); //如果傳錯檔案格式就會反彈回去
                }

                //把檔案轉成二進位，並放入byte[]
                var mem = new MemoryStream();

                uploadPhoto.CopyTo(mem);

                post.Photos = mem.ToArray(); //轉成陣列
                post.ImageType = uploadPhoto.ContentType;

            }

            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now; 
                await _context.AddAsync(post); // 使用 AddAsync 方法
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
            var post = await _context.Post
                .Include(p => p.RePost) // 加載與 Post 相關的 RePost
                .FirstOrDefaultAsync(m => m.PostID == id);

            if (post != null)
            {
                // 刪除所有與該文章相關的留言
                _context.RePost.RemoveRange(post.RePost); // 刪除 RePost

                // 刪除文章本身
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
