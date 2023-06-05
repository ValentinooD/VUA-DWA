using DAL.Mappers;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RwaMoviesContext>(options =>
{
    options.UseSqlServer("name=ConnectionStrings:RWAConnStr");
});

//builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddAutoMapper(
    typeof(Program),
    typeof(DAL.Mappers.AutomapperProfile),
    typeof(AdminModule.Mappers.AutomapperProfile));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("API");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Videos}/{action=Index}/{id?}");

app.Run();
