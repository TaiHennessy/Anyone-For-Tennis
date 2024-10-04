using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AnyoneForTennis.Data;
using System;
using System.Linq;

namespace AnyoneForTennis.Models;

public static class SeedData
{
    public static async void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new LocalDbContext(serviceProvider.GetRequiredService<DbContextOptions<LocalDbContext>>()))
        {
            // Identify any Users
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }
            context.Users.AddRange(
                new User
                {
                    Username = "Big Boy",
                    Password = "safepassword",
                    IsAdmin = true
                },

                new User
                {
                    Username = "John Smith",
                    Password = "123456",
                    IsAdmin = false
                },

                new User
                {
                    Username = "No Name",
                    Password = "No Password",
                    IsAdmin = false
                },

                new User
                {
                    Username = "Luigi Mortadella",
                    Password = "parmesan",
                    IsAdmin = false
                }


            );
            await context.SaveChangesAsync();
        }
    }
}


