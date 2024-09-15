using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolProject_DB.Controllers
{
    public class LoginController : Controller
    {
        private readonly SchoolProjectContext _context;

        public LoginController(SchoolProjectContext context)
        {
            _context = context;
        }

        // 登入動作
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "Email 和 密碼 都必須填寫";
                return View();
            }

            // 檢查使用者是否存在
            var member = _context.Members
                .FirstOrDefault(m => m.Email == email && m.Password == password);

            if (member != null)
            {
                // 創建使用者的身份聲明
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, member.Email),
                    new Claim("IsAdmin", member.IsAdmin.ToString())
                };

                // 創建身份驗證資料
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 設定 Cookie，並將閒置時間設為 30 分鐘
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true // 仍然設置 cookie 持久化
                        // 不再需要手動設置 SlidingExpiration
                    });


                // 將 MemberID 儲存到 Cookie 中，但不設定過期時間，表示瀏覽器關閉時清除
                Response.Cookies.Append("MemberID", member.MemberID, new CookieOptions
                {
                    HttpOnly = true, // 設置 HttpOnly 為 true，防止 JavaScript 訪問
                    SameSite = SameSiteMode.Lax, // 允許跨站請求時使用
                    Secure = false // 測試環境設為 false，生產環境應設為 true 並使用 HTTPS
                    // 不設置 Expires，這樣 cookie 在瀏覽器關閉時自動清除
                });

                return RedirectToAction("Index", "Home"); // 登入成功後導向首頁
            }
            else
            {
                ViewData["Error"] = "登入失敗，請檢查 帳號 或 密碼";
                return View();
            }
        }

        // 登入頁面
        public IActionResult Login()
        {
            return View();
        }

        // 登出動作
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // 清除 Cookie 資料並登出
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 清除自定義的 MemberID Cookie
            Response.Cookies.Delete("MemberID");

            // 重定向到首頁
            return RedirectToAction("Index", "Home");
        }
    }
}
