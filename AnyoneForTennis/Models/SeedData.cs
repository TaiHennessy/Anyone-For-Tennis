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
            if (!context.Users.Any())
            {
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
                    Description = "Defense is the best Offence"
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


