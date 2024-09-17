using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;

var builder = WebApplication.CreateBuilder(args);

// 在Program.cs加入使用appsettings.json中的連線字串程式碼
builder.Services.AddDbContext<SchoolProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolProjectConnection")));

// 註冊 HttpClient 服務，用於爬蟲功能
builder.Services.AddHttpClient();

// 設置 Session 支持
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設置 Session 閒置過期時間
    options.Cookie.HttpOnly = true; // 避免前端 JavaScript 訪問
    options.Cookie.IsEssential = true; // 支持 GDPR 合規
});

// 配置身份驗證（Cookie 基於 HTTPS 的驗證）
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 確保 Cookie 僅在 HTTPS 上發送
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // 確保 Session 在路由和授權中介軟體之前啟用
app.UseAuthentication(); // 確保身份驗證啟用
app.UseAuthorization(); // 啟用授權

// 設定控制器路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
