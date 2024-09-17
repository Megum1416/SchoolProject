using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;
using HtmlAgilityPack; // 引入HtmlAgilityPack，用來解析網頁內容
using System.Net.Http; // 引入HttpClient，用來發送HTTP請求
using System.IO; // 用於處理檔案操作

namespace SchoolProject_DB.Controllers
{
    public class MembersController : Controller
    {
        private readonly SchoolProjectContext _context; // 資料庫上下文
        private readonly HttpClient _httpClient; // 用於發送HTTP請求的工具

        public MembersController(SchoolProjectContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            var members = await _context.Members.ToListAsync();
            return View(members);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateIsAdmin(string id, bool isAdmin)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return Json(new { success = false });
            }

            member.IsAdmin = isAdmin;
            _context.Update(member);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        // 顯示所有會員的列表
        public async Task<IActionResult> Index()
        {
            var members = await _context.Members.ToListAsync();

            // 將 IsAdmin 狀態存入 ViewBag，檢查 Session 是否存在
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");

            return View(members);
        }

        // 顯示某個會員的詳細資料
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var members = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == id);
            if (members == null) return NotFound();

            // 從 Session 中取得 MemberID
            var sessionMemberID = HttpContext.Session.GetString("MemberID");
            ViewBag.SessionMemberID = sessionMemberID;

            return View(members);
        }



        // 進入會員創建頁面
        public async Task<IActionResult> Create()
        {
            // 找到當前資料庫中最大的 MemberID
            var maxMemberID = await _context.Members
                                            .OrderByDescending(m => m.MemberID)
                                            .Select(m => m.MemberID)
                                            .FirstOrDefaultAsync();

            // 如果資料庫是空的，預設 MemberID 從 00000001 開始
            string newMemberID = "00000001";
            if (!string.IsNullOrEmpty(maxMemberID))
            {
                // 轉換為數字，加 1 後轉回 8 位數字的字串
                newMemberID = (int.Parse(maxMemberID) + 1).ToString("D8");
            }

            // 將新的 MemberID 存入 Session
            HttpContext.Session.SetString("NewMemberID", newMemberID);

            // 將 newMemberID 傳遞到視圖
            ViewBag.NewMemberID = newMemberID;

            return View();
        }




        // 當使用者提交會員創建表單時處理的動作
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberID,Email,Password,UserName,LodestoneID,FirstName,FamilyName,DataCenter,ServerName,CreatedAt,UpdatedAt,Photos,ImageType")] Members members, IFormFile? uploadPhoto, string? PhotoUrl)
        {
            // 如果MemberID未正確從表單接收，從Session中設置
            if (string.IsNullOrEmpty(members.MemberID))
            {
                members.MemberID = HttpContext.Session.GetString("NewMemberID");
                if (string.IsNullOrEmpty(members.MemberID))
                {
                    ModelState.AddModelError("", "MemberID未生成，請重新嘗試。");
                    return View(members);
                }
            }

            // 檢查資料庫是否已經有相同的 LodestoneID
            var existingMember = await _context.Members.FirstOrDefaultAsync(m => m.LodestoneID == members.LodestoneID);
            if (existingMember != null)
            {
                ModelState.AddModelError("LodestoneID", "此 LodestoneID 已經存在，請使用不同的 ID。");
                return View(members); // 返回原始表單並顯示錯誤訊息
            }

            await HandlePhotoUpload(members, uploadPhoto, PhotoUrl);

            if (ModelState.IsValid)
            {
                members.CreatedAt = DateTime.Now;
                _context.Add(members);
                await _context.SaveChangesAsync();

                // 成功後導向 Login 頁面
                return RedirectToAction("Login", "Login");
            }

            return View(members);
        }



        // 用於給使用者創建資料時，抓取角色資訊的action
        [HttpGet]
        public async Task<IActionResult> FetchLodestoneInfo(string lodestoneID)
        {
            if (!string.IsNullOrEmpty(lodestoneID))
            {
                string url = $"https://na.finalfantasyxiv.com/lodestone/character/{lodestoneID}/";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);

                    // 解析角色名稱
                    var nameNode = htmlDoc.DocumentNode.SelectSingleNode("//p[@class='frame__chara__name']");
                    string firstName = null;
                    string familyName = null;
                    if (nameNode != null)
                    {
                        var nameParts = HtmlEntity.DeEntitize(nameNode.InnerText).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (nameParts.Length >= 2)
                        {
                            firstName = nameParts[1].Trim();
                            familyName = nameParts[0].Trim();
                        }
                    }

                    // 解析伺服器資訊
                    var serverInfoNode = htmlDoc.DocumentNode.SelectSingleNode("//p[@class='frame__chara__world']");
                    string dataCenter = null;
                    string serverName = null;
                    if (serverInfoNode != null)
                    {
                        var serverInfo = HtmlEntity.DeEntitize(serverInfoNode.InnerText).Split('[');
                        serverName = serverInfo[0].Trim();
                        dataCenter = serverInfo[1].Replace("]", "").Trim();
                    }

                    var data = new
                    {
                        firstName,
                        familyName,
                        serverName,
                        dataCenter
                    };

                    return Json(data);
                }
            }

            return Json(null);
        }


        // 用於給使用者創建資料時，抓取角色照片的action
        [HttpGet]
        public async Task<IActionResult> FetchLodestonePhoto(string lodestoneID)
        {
            if (!string.IsNullOrEmpty(lodestoneID))
            {
                string url = $"https://na.finalfantasyxiv.com/lodestone/character/{lodestoneID}/";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);

                    // 爬取照片的URL
                    var photoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='character__detail__image']//img");
                    string photoUrl = null;
                    if (photoNode != null)
                    {
                        photoUrl = photoNode.GetAttributeValue("src", null);
                    }

                    var data = new
                    {
                        photoUrl
                    };

                    return Json(data);
                }
            }

            return Json(null);
        }

        // 進入會員資料編輯頁面
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var members = await _context.Members.FindAsync(id);
            if (members == null) return NotFound();

            return View(members);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MemberID,Password,UserName,FirstName,FamilyName,DataCenter,ServerName,Photos,ImageType")] Members members, IFormFile? uploadPhoto, string? PhotoUrl)
        {
            if (id != members.MemberID) return NotFound();

            await HandlePhotoUpload(members, uploadPhoto, PhotoUrl);

            if (ModelState.IsValid)
            {
                // 保護 IsAdmin 欄位不被修改
                _context.Entry(members).Property(m => m.IsAdmin).IsModified = false;

                // 自動更新 UpdatedAt
                members.UpdatedAt = DateTime.Now;

                _context.Update(members);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(members);
        }


        // 讀取會員的照片+使用者沒上傳圖片時要顯示預設
        public async Task<FileContentResult> GetImage(string memberId)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == memberId);

            if (member == null || member.Photos == null)
            {
                var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/default-photo.jpg");
                var defaultImage = await System.IO.File.ReadAllBytesAsync(defaultImagePath);
                return File(defaultImage, "image/jpeg");
            }

            return File(member.Photos, member.ImageType ?? "image/jpeg");
        }

        // 新增批量更新所有會員圖片的功能
        [HttpPost]
        public async Task<IActionResult> ScrapeAllPhotos()
        {
            var members = await _context.Members.ToListAsync();
            foreach (var member in members)
            {
                if (!string.IsNullOrEmpty(member.LodestoneID))
                {
                    string photoUrl = await FetchLodestonePhotoForMember(member.LodestoneID);
                    if (!string.IsNullOrEmpty(photoUrl))
                    {
                        var photoBytes = await DownloadPhoto(photoUrl);
                        if (photoBytes != null)
                        {
                            member.Photos = photoBytes;
                            member.ImageType = "image/jpeg"; // 假設圖片為jpeg格式
                            _context.Update(member);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //此action是拿來配合上面action做使用的
        private async Task<string> FetchLodestonePhotoForMember(string lodestoneID)
        {
            if (!string.IsNullOrEmpty(lodestoneID))
            {
                string url = $"https://na.finalfantasyxiv.com/lodestone/character/{lodestoneID}/";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);

                    var photoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='character__detail__image']//img");
                    if (photoNode != null)
                    {
                        return photoNode.GetAttributeValue("src", null);
                    }
                }
            }
            return null;
        }

        private async Task<byte[]> DownloadPhoto(string photoUrl)
        {
            var response = await _httpClient.GetAsync(photoUrl);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }

        private async Task HandlePhotoUpload(Members members, IFormFile? uploadPhoto, string? photoUrl)
        {
            if (uploadPhoto != null)
            {
                if (uploadPhoto.ContentType == "image/jpeg" || uploadPhoto.ContentType == "image/png")
                {
                    var mem = new MemoryStream();
                    await uploadPhoto.CopyToAsync(mem);
                    members.Photos = mem.ToArray();
                    members.ImageType = uploadPhoto.ContentType;
                }
                else
                {
                    ModelState.AddModelError("", "僅能上傳JPG或PNG檔。");
                }
            }
            else if (!string.IsNullOrEmpty(photoUrl))
            {
                var photoBytes = await DownloadPhoto(photoUrl);
                if (photoBytes != null)
                {
                    members.Photos = photoBytes;
                    members.ImageType = "image/jpeg"; // 假設抓取的圖片是jpeg格式
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCroppedPhoto(IFormFile croppedPhoto)
        {
            if (croppedPhoto != null && (croppedPhoto.ContentType == "image/jpeg" || croppedPhoto.ContentType == "image/png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await croppedPhoto.CopyToAsync(memoryStream);
                    var member = await _context.Members.FindAsync(/* 用戶ID或其他標識 */);
                    if (member != null)
                    {
                        member.Photos = memoryStream.ToArray();
                        member.ImageType = croppedPhoto.ContentType;
                        _context.Update(member);
                        await _context.SaveChangesAsync();

                        return Json(new { success = true });
                    }
                }
            }

            return Json(new { success = false });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // 如果ID為空，回傳404
            }

            var member = await _context.Members.FindAsync(id); // 查找對應的會員
            if (member == null)
            {
                return NotFound(); // 如果會員不存在，回傳404
            }

            _context.Members.Remove(member); // 刪除該會員
            await _context.SaveChangesAsync(); // 保存更改到資料庫

            return RedirectToAction(nameof(Index)); // 刪除後重定向回列表頁
        }

        private bool MemberExists(string id)
        {
            return _context.Members.Any(e => e.MemberID == id);
        }


    }
}