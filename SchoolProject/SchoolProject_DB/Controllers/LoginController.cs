
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;
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
                ViewData["Error"] = "帳號 和 密碼 都必須填寫";
                return View();
            }

            // 檢查使用者是否存在
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email == email && m.Password == password);

            if (member != null)
            {
                // 登入成功，將 MemberID 和 IsAdmin 儲存到 Session
                HttpContext.Session.SetString("MemberID", member.MemberID);
                HttpContext.Session.SetString("Email", member.Email);
                HttpContext.Session.SetString("IsAdmin", member.IsAdmin.ToString());
                HttpContext.Session.SetString("UserName", member.UserName);
                HttpContext.Session.SetString("LodestoneID", member.LodestoneID);


                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["Error"] = "登入失敗，請檢查帳號或密碼";
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
        public IActionResult Logout()
        {
            // 清除 Session 資料
            HttpContext.Session.Clear();

            // 重定向到首頁
            return RedirectToAction("Index", "Home");
        }
    }
}
