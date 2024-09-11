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
            // 從 Cookie 中取得當前登入使用者的 MemberID
            var currentUserId = Request.Cookies["MemberID"];
            if (string.IsNullOrEmpty(currentUserId))
            {
                // 如果未找到 MemberID，跳轉至登入頁面
                return RedirectToAction("Login", "LoginController");
            }

            // 查詢當前使用者的追蹤名單
            var followList = await _context.FollowList
                .Include(f => f.Member) // 包含會員資訊
                .Where(f => f.MemberID == currentUserId)
                .ToListAsync();

            // 如果追蹤列表為空，顯示訊息
            if (!followList.Any())
            {
                ViewBag.Message = "您目前尚未追蹤任何人。";
            }

            return View(followList);
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


        [HttpPost]
        public IActionResult Follow(string followedMemberId)
        {
            try
            {
                // 從 Cookie 取得當前使用者的 MemberID
                var currentUserId = Request.Cookies["MemberID"];

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "未授權，無法找到有效的 MemberID" });
                }

                // 找到被追蹤的會員 (MemberID = 00000002)
                var followedMember = _context.Members
                    .FirstOrDefault(m => m.MemberID == followedMemberId);

                if (followedMember == null)
                {
                    return Json(new { success = false, message = "找不到追蹤的會員。" });
                }

                // 檢查是否已經追蹤該會員
                var existingFollow = _context.FollowList
                    .FirstOrDefault(f => f.MemberID == currentUserId && f.LodestoneID == followedMember.LodestoneID);

                if (existingFollow != null)
                {
                    // 已經追蹤，則執行刪除操作
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
                    string newFollowID;
                    if (string.IsNullOrEmpty(maxFollowID))
                    {
                        newFollowID = "00000001"; // 如果沒有任何資料，預設為 "00000001"
                    }
                    else
                    {
                        int nextFollowID = int.Parse(maxFollowID) + 1; // 取最大 FollowID + 1
                        newFollowID = nextFollowID.ToString("D8"); // 格式化為 8 位數
                    }

                    // 建立新的 FollowList 資料
                    var newFollow = new FollowList
                    {
                        FollowID = newFollowID, // 自動生成的 FollowID
                        MemberID = currentUserId, // 當前登入的使用者
                        LodestoneID = followedMember.LodestoneID, // 被追蹤會員的 LodestoneID
                        CreatedAt = DateTime.Now // 當下時間
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
                // 從 Cookie 取得當前使用者的 MemberID
                var currentUserId = Request.Cookies["MemberID"];

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { isFollowing = false });
                }

                // 找到被追蹤的會員
                var followedMember = _context.Members
                    .FirstOrDefault(m => m.MemberID == followedMemberId);

                if (followedMember == null)
                {
                    return Json(new { isFollowing = false });
                }

                // 直接返回 Any() 的結果，無需定義變量
                return Json(new
                {
                    isFollowing = _context.FollowList
                        .Any(f => f.MemberID == currentUserId && f.LodestoneID == followedMember.LodestoneID)
                });
            }
            catch (Exception)
            {
                return Json(new { isFollowing = false });
            }
        }
    }
}
