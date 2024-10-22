using AnyoneForTennis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure the Hitdb1Context for production data
builder.Services.AddDbContext<Hitdb1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection")));

// Configure the LocalDbContext for local data (including Identity data)
builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services (including UserManager and SignInManager)
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<LocalDbContext>()
    .AddDefaultTokenProviders();

// Configure the login path to point to Registration/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Registration/Login";   // Set login redirection path
    options.LogoutPath = "/Home/Logout";         // Set logout handling path
    options.AccessDeniedPath = "/Home/AccessDenied"; // Set access denied redirection
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie expiration time
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout duration
    options.Cookie.HttpOnly = true; // Set the cookie to be HttpOnly
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

// Add controllers and views
builder.Services.AddControllersWithViews(options =>
{
    // Enforce authentication globally for all controllers/actions by default
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

var app = builder.Build();

// Seed Data Initializer
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services); // Await the seeding method
}


/* OG Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
} */

//New Error handling middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Shows detailed error information in development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redirects to Error action in HomeController
    app.UseHsts(); // HTTP Strict Transport Security (HSTS) is enforced
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use Identity services
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();  // Enable authorization middleware

app.UseSession();

// Map default route and allow anonymous access to Login and Register pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Route for simulated error
app.MapControllerRoute(
    name: "simulateError",
    pattern: "Home/SimulatedError",
    defaults: new { controller = "Home", action = "SimulatedError" });

// Run the application
app.Run();
