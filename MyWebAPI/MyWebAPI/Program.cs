using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//2.1.5 在Program.cs加入使用appsettings.json中的連線字串程式碼(這段必須寫在var builder這行後面)
builder.Services.AddDbContext<SchoolProjectContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolProjectConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();

app.Run();
