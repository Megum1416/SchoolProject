using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;

var builder = WebApplication.CreateBuilder(args);

// �bProgram.cs�[�J�ϥ�appsettings.json�����s�u�r��{���X
builder.Services.AddDbContext<SchoolProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolProjectConnection")));

// ���U HttpClient �A�ȡA�Ω��Υ\��
builder.Services.AddHttpClient();

// �]�m Session ���
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // �]�m Session ���m�L���ɶ�
    options.Cookie.HttpOnly = true; // �קK�e�� JavaScript �X��
    options.Cookie.IsEssential = true; // ��� GDPR �X�W
});

// �t�m�������ҡ]Cookie ��� HTTPS �����ҡ^
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // �T�O Cookie �Ȧb HTTPS �W�o�e
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

app.UseSession(); // �T�O Session �b���ѩM���v�����n�餧�e�ҥ�
app.UseAuthentication(); // �T�O�������ұҥ�
app.UseAuthorization(); // �ҥα��v

// �]�w�������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
