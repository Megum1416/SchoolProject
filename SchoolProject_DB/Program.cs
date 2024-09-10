using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;

var builder = WebApplication.CreateBuilder(args);

//6.1.4 在Program.cs加入使用appsettings.json中的連線字串程式碼
builder.Services.AddDbContext<SchoolProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolProjectConnection")));

// 註冊 HttpClient 服務，用於爬蟲功能
builder.Services.AddHttpClient();

// 配置 Cookie-based Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // 指定登入頁面的路徑
        options.LogoutPath = "/Logout"; // 指定登出頁面的路徑
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 設定閒置30分鐘過期
        options.SlidingExpiration = true; // 啟用滑動過期
    });


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// 啟用 Cookie-based Authentication
app.UseAuthentication();
app.UseAuthorization();

// 設定控制器路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
