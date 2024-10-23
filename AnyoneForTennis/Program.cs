using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure the Hitdb1Context for production data
builder.Services.AddDbContext<Hitdb1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection")));

// Configure the LocalDbContext for local data (including Identity data)
builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<LocalDbContext>()
    .AddDefaultTokenProviders();

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Registration/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed Data Initializer
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services); // Await the seeding method
}

// Create a logger
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure global error handling middleware
if (!app.Environment.IsDevelopment())
{
    // Use the custom error handler for non-development environments
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Show detailed error pages in development
    app.UseDeveloperExceptionPage();
}

// Handle 404 and other status code pages explicitly
app.UseStatusCodePages(async context =>
{
    var statusCode = context.HttpContext.Response.StatusCode;
    if (statusCode == 404)
    {
        // Log the 404 error as an error in the console
        var path = context.HttpContext.Request.Path;
        logger.LogError("404 Error - Page Not Found: {Path}", path);

        // Redirect to the custom error page
        context.HttpContext.Response.Redirect("/Home/Error");
    }
    else
    {
        // Handle other status codes here if needed
        logger.LogError("Error - Status Code: {StatusCode} at {Path}", statusCode, context.HttpContext.Request.Path);
    }
});

// Middleware pipeline configuration
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Map default routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run the application
app.Run();
