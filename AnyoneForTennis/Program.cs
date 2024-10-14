using AnyoneForTennis.Data;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<Hitdb1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection")));

builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout duration
    options.Cookie.HttpOnly = true; // Set the cookie to be HttpOnly
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

// Seed Data Initializer
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services); // Await the seeding method
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Homepage}/{id?}");

app.Run();
