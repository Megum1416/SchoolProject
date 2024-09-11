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
            return View(await _context.Members.ToListAsync());
        }

        // 顯示某個會員的詳細資料
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var members = await _context.Members.FirstOrDefaultAsync(m => m.MemberID == id);
            if (members == null) return NotFound();

            return View(members);
        }

        // 進入會員創建頁面
        public async Task<IActionResult> Create()
        {
            // 找到當前資料庫中最大的MemberID
            var maxMemberID = await _context.Members
                                            .OrderByDescending(m => m.MemberID)
                                            .Select(m => m.MemberID)
                                            .FirstOrDefaultAsync();

            // 如果資料庫是空的，預設MemberID從00000001開始
            string newMemberID = "00000001";
            if (!string.IsNullOrEmpty(maxMemberID))
            {
                // 轉換為數字，加1後轉回8位數字的字串
                newMemberID = (int.Parse(maxMemberID) + 1).ToString("D8");
            }

            // 將新的MemberID存入Cookie，有效期設置為短時間，例如1小時
            Response.Cookies.Append("NewMemberID", newMemberID, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddHours(1)
            });

            return View();
        }

        // 當用戶提交會員創建表單時處理的動作
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberID,Email,Password,UserName,LodestoneID,FirstName,FamilyName,DataCenter,ServerName,CreatedAt,UpdatedAt,Photos,ImageType")] Members members, IFormFile? uploadPhoto, string? PhotoUrl)
        {
            if (string.IsNullOrEmpty(members.MemberID))
            {
                ModelState.AddModelError("", "MemberID未生成，請重新嘗試。");
                return View(members);
            }

            await HandlePhotoUpload(members, uploadPhoto, PhotoUrl);

            if (ModelState.IsValid)
            {
                members.CreatedAt = DateTime.Now;
                _context.Add(members);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(members);
        }

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


        // 用於抓取角色照片的動作方法
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
        public async Task<IActionResult> Edit(string id, [Bind("Password,UserName,FirstName,FamilyName,DataCenter,ServerName,Photos,ImageType")] Members members, IFormFile? uploadPhoto, string? PhotoUrl)
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


        // 讀取會員的照片
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


        private bool MembersExists(string id)
        {
            return _context.Members.Any(e => e.MemberID == id);
        }
    }
}