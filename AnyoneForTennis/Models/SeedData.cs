using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AnyoneForTennis.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnyoneForTennis.Models
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new LocalDbContext(serviceProvider.GetRequiredService<DbContextOptions<LocalDbContext>>()))
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // Identify any Users
                if (!context.Users.Any())
                {
                    var users = new[]
                    {
                        new { Username = "Big Boy", Password = "safepassword", IsAdmin = true },
                        new { Username = "John Smith", Password = "123456", IsAdmin = false },
                        new { Username = "No Name", Password = "No Password", IsAdmin = false },
                        new { Username = "Luigi Mortadella", Password = "parmesan", IsAdmin = false }
                    };

                    foreach (var u in users)
                    {
                        var identityUser = new IdentityUser { UserName = u.Username, Email = $"{u.Username}@example.com" };
                        var result = await userManager.CreateAsync(identityUser, u.Password);

                        if (result.Succeeded && u.IsAdmin)
                        {
                            await userManager.AddToRoleAsync(identityUser, "Admin");
                        }
                    }
                }

                // Seeding Schedule Data - Looks for any schedules
                if (!context.Schedule.Any())
                {
                    context.Schedule.AddRange(
                    new Schedule
                    {
                        Name = "Super Tennis Training",
                        Location = "Court D",
                        Description = "Training for Winners"
                    },
                    new Schedule
                    {
                        Name = "Defensive Tennis Drills",
                        Location = "Court A",
                        Description = "Defense is the best Offense"
                    },
                    new Schedule
                    {
                        Name = "Tennis for Beginners",
                        Location = "Court C",
                        Description = "Training for Beginners"
                    },
                    new Schedule
                    {
                        Name = "Ultra Marathon Tennis",
                        Location = "Court B",
                        Description = "Not for the weak willed"
                    }
                );
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
