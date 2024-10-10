using AnyoneForTennis.Data;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the production context (read-only) using the ProductionConnection
builder.Services.AddDbContext<Hitdb1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection")));

// Register the local context (for local features) using the DefaultConnection
builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers with views
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout duration
    options.Cookie.HttpOnly = true; // Set the cookie to be HttpOnly
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

// Seed Data Initialiser

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session middleware
app.UseSession();

app.UseAuthorization();



// Map default routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();