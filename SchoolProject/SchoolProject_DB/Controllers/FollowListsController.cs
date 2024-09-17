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
    public class FollowListsController : Controller
    {
        private readonly SchoolProjectContext _context;

        public FollowListsController(SchoolProjectContext context)
        {
            _context = context;
        }


        // GET: FollowLists/Index
        public async Task<IActionResult> Index()
        {
            // 從 Session 中取得當前登入使用者的 MemberID
            var currentUserId = HttpContext.Session.GetString("MemberID");
            if (string.IsNullOrEmpty(currentUserId))
            {
                // 如果未找到 MemberID，跳轉至登入頁面
                return RedirectToAction("Login", "LoginController");
            }

            // 查詢當前使用者的追蹤名單，並根據 LodestoneID 取得對應的會員資料
            var followedMembers = await _context.FollowList
                .Where(f => f.MemberID == currentUserId)
                .Join(_context.Members,
                      follow => follow.LodestoneID,
                      member => member.LodestoneID,
                      (follow, member) => member) // 只選擇會員資料
                .ToListAsync();

            // 如果追蹤列表為空，顯示訊息
            if (!followedMembers.Any())
            {
                ViewBag.Message = "您目前尚未追蹤任何人。";
            }

            return View(followedMembers);
        }


        /*
        // GET: FollowLists/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followList = await _context.FollowList
                .Include(f => f.Member)
                .FirstOrDefaultAsync(m => m.FollowID == id);
            if (followList == null)
            {
                return NotFound();
            }

            return View(followList);
        }

        // GET: FollowLists/Create
        public IActionResult Create()
        {
            ViewData["MemberID"] = new SelectList(_context.Members, "MemberID", "MemberID");
            return View();
        }

        // POST: FollowLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FollowID,MemberID,LodestoneID,CreatedAt,UpdatedAt")] FollowList followList)
        {
            if (ModelState.IsValid)
            {
                _context.Add(followList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberID"] = new SelectList(_context.Members, "MemberID", "MemberID", followList.MemberID);
            return View(followList);
        }

        // GET: FollowLists/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followList = await _context.FollowList.FindAsync(id);
            if (followList == null)
            {
                return NotFound();
            }
            ViewData["MemberID"] = new SelectList(_context.Members, "MemberID", "MemberID", followList.MemberID);
            return View(followList);
        }

        // POST: FollowLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FollowID,MemberID,LodestoneID,CreatedAt,UpdatedAt")] FollowList followList)
        {
            if (id != followList.FollowID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowListExists(followList.FollowID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberID"] = new SelectList(_context.Members, "MemberID", "MemberID", followList.MemberID);
            return View(followList);
        }
       

        // GET: FollowLists/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followList = await _context.FollowList
                .Include(f => f.Member)
                .FirstOrDefaultAsync(m => m.FollowID == id);
            if (followList == null)
            {
                return NotFound();
            }

            return View(followList);
        }

         */

        //這裡的delete action比較適合給管理者在管理所有資料時使用的。
        // POST: FollowLists/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound(); // 如果ID為空，回傳404錯誤
            }

            // 根據ID查詢要刪除的追蹤記錄
            var followList = await _context.FollowList.FindAsync(id);
            if (followList == null)
            {
                return NotFound(); // 如果找不到對應的記錄，回傳404錯誤
            }

            // 將找到的追蹤記錄從資料庫中移除
            _context.FollowList.Remove(followList);
            await _context.SaveChangesAsync(); // 保存資料庫變更

            // 返回到追蹤列表頁面並顯示成功訊息
            return RedirectToAction(nameof(Index));
        }

        private bool FollowListExists(string id)
        {
            return _context.FollowList.Any(e => e.FollowID == id);
        }

        // 從 Session 中取得當前使用者的 MemberID
        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("MemberID");
        }

        // 檢查會員是否存在
        private Members GetFollowedMember(string memberId)
        {
            return _context.Members.FirstOrDefault(m => m.MemberID == memberId);
        }

        //這裡的delete是適合給使用者在操作時的
        [HttpPost]
        public IActionResult Follow(string followedMemberId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "未授權，無法找到有效的 MemberID" });
                }

                var followedMember = GetFollowedMember(followedMemberId);
                if (followedMember == null)
                {
                    return Json(new { success = false, message = "找不到追蹤的會員。" });
                }

                // 檢查是否已經追蹤該會員
                var existingFollow = _context.FollowList
                    .FirstOrDefault(f => f.MemberID == currentUserId && f.LodestoneID == followedMember.LodestoneID);

                if (existingFollow != null)
                {
                    // 已經追蹤，則執行取消追蹤操作
                    _context.FollowList.Remove(existingFollow);
                    _context.SaveChanges();
                    return Json(new { success = true, action = "unfollowed" });
                }
                else
                {
                    // 查詢最大 FollowID
                    var maxFollowID = _context.FollowList
                        .OrderByDescending(f => f.FollowID)
                        .Select(f => f.FollowID)
                        .FirstOrDefault();

                    // 設置新的 FollowID，預設為 "00000001" 並處理加 1 的邏輯
                    string newFollowID = string.IsNullOrEmpty(maxFollowID)
                        ? "00000001"
                        : (int.Parse(maxFollowID) + 1).ToString("D8");

                    // 建立新的 FollowList 資料
                    var newFollow = new FollowList
                    {
                        FollowID = newFollowID,
                        MemberID = currentUserId,
                        LodestoneID = followedMember.LodestoneID,
                        CreatedAt = DateTime.Now
                    };

                    _context.FollowList.Add(newFollow);
                    _context.SaveChanges();

                    return Json(new { success = true, action = "followed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult CheckFollowStatus(string followedMemberId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { isFollowing = false });
                }

                var followedMember = GetFollowedMember(followedMemberId);
                if (followedMember == null)
                {
                    return Json(new { isFollowing = false });
                }

                // 檢查當前使用者是否已經追蹤該會員
                var isFollowing = _context.FollowList
                    .Any(f => f.MemberID == currentUserId && f.LodestoneID == followedMember.LodestoneID);

                return Json(new { isFollowing });
            }
            catch (Exception)
            {
                return Json(new { isFollowing = false });
            }
        }


    }
}
