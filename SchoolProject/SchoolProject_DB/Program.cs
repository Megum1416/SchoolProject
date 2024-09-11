using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;

var builder = WebApplication.CreateBuilder(args);

//6.1.4 �bProgram.cs�[�J�ϥ�appsettings.json�����s�u�r��{���X
builder.Services.AddDbContext<SchoolProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolProjectConnection")));

// ���U HttpClient �A�ȡA�Ω��Υ\��
builder.Services.AddHttpClient();

// �t�m Cookie-based Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // ���w�n�J���������|
        options.LogoutPath = "/Logout"; // ���w�n�X���������|
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // �]�w���m30�����L��
        options.SlidingExpiration = true; // �ҥηưʹL��
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

// �ҥ� Cookie-based Authentication
app.UseAuthentication();
app.UseAuthorization();

// �]�w�������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
