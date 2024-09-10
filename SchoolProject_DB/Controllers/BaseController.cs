using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolProject_DB.Models;
using System.Linq;

public class BaseController : Controller
{
    private readonly SchoolProjectContext _context;

    public BaseController(SchoolProjectContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        if (User.Identity.IsAuthenticated)
        {
            // 從 cookie 中獲取 MemberID
            var memberID = Request.Cookies["MemberID"];
            Console.WriteLine($"從 Cookie 獲取的 MemberID: {memberID}");

            if (!string.IsNullOrEmpty(memberID))
            {
                // 查詢資料庫中的會員
                var member = _context.Members.FirstOrDefault(m => m.MemberID == memberID);

                if (member != null)
                {
                    // 將會員的 UserName 存入 ViewBag，傳遞給 View
                    ViewBag.UserName = member.UserName;
                    Console.WriteLine($"會員暱稱：{member.UserName}");
                }
                else
                {
                    Console.WriteLine("未找到對應的會員");
                }
            }
            else
            {
                Console.WriteLine("Cookie 中沒有 MemberID");
            }
        }
        else
        {
            Console.WriteLine("用戶未登入");
        }
    }
}
