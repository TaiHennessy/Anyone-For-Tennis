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
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var coachContext = new Hitdb1Context(
                serviceProvider.GetRequiredService<DbContextOptions<Hitdb1Context>>()))
            using (var localContext = new LocalDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<LocalDbContext>>()))
            {
                Console.WriteLine("SeedData: Initializing data...");

                // Ensure the local database is created
                await localContext.Database.EnsureCreatedAsync();

                // Fetch coaches from Hitdb1Context (source database)
                var coachesFromSource = await coachContext.Coaches
                    .FromSqlRaw("SELECT CoachId, FirstName, LastName, Biography, Photo FROM dbo.Coaches")
                    .ToListAsync();

                Console.WriteLine($"SeedData: Number of coaches fetched from source: {coachesFromSource.Count}");

                // Fetch coaches from the local database
                var localCoaches = await localContext.Coach.ToListAsync();
                Console.WriteLine($"SeedData: Number of coaches in local database: {localCoaches.Count}");

                // Sync the coaches data between source and local database
                await SyncCoaches(coachesFromSource, localCoaches, localContext);

                // Seed users if none exist
                if (!await localContext.Users.AnyAsync())
                {
                    await SeedUsersAsync(localContext);
                }

                // Seed schedules if none exist
                if (!await localContext.Schedule.AnyAsync())
                {
                    // Get coach from local seeded coach
                    var coach1 = await localContext.Coach.FirstOrDefaultAsync(c => c.FirstName == "Jane" && c.LastName == "Johnson");
                    var coach2 = await localContext.Coach.FirstOrDefaultAsync(c => c.FirstName == "David" && c.LastName == "Miller");

                    // Compressed the seeded stuff less painful to work with
                    var seededSchedules = new[]
                    {
                        new Schedule {Name = "Super Tennis Training", Location = "Court D",Description = "Training for Winners"},
                        new Schedule {Name = "Defensive Tennis Drills", Location = "Court A", Description = "Defense is the best Offence"},
                        new Schedule {Name = "Tennis for Beginners", Location = "Court C", Description = "Training for Beginners"},
                        new Schedule {Name = "Ultra Marathon Tennis", Location = "Court B", Description = "Not for the weak willed"}
                    };

                    // Saves the schedules so that schedule variables can reference correctly
                    localContext.Schedule.AddRange(seededSchedules);
                    await localContext.SaveChangesAsync();

                    // Getting the Schedules from the Seeded Data
                    var schedule1 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Super Tennis Training");
                    var schedule2 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Defensive Tennis Drills");
                    var schedule3 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Tennis for Beginners");
                    var schedule4 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Ultra Marathon Tennis");

                    localContext.SchedulePlus.AddRange(
                        new SchedulePlus {ScheduleId = schedule1.ScheduleId, CoachId = coach1.CoachId, DateTime = new DateTime(2024, 11, 11, 12, 44, 00)},
                        new SchedulePlus {ScheduleId = schedule2.ScheduleId, CoachId = coach2.CoachId, DateTime = new DateTime(2024, 05, 02, 11, 50, 00)},
                        new SchedulePlus {ScheduleId = schedule3.ScheduleId, CoachId = coach1.CoachId, DateTime = new DateTime(2024, 10, 09, 12, 55, 55)},
                        new SchedulePlus {ScheduleId = schedule4.ScheduleId, CoachId = coach2.CoachId, DateTime = new DateTime(2024, 01, 04, 05, 06, 07)}
                    );
                    await localContext.SaveChangesAsync();
                }
            }
        }

        private static async Task SyncCoaches(
            List<Coach> coachesFromSource,
            List<Coach> localCoaches,
            LocalDbContext localContext)
        {
            using (var transaction = await localContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Enable IDENTITY_INSERT for dbo.Coach
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Coach ON;");

                    // Update or add coaches
                    foreach (var sourceCoach in coachesFromSource)
                    {
                        var existingCoach = localCoaches.FirstOrDefault(c => c.CoachId == sourceCoach.CoachId);

                        if (existingCoach != null)
                        {
                            if (existingCoach.FirstName != sourceCoach.FirstName ||
                                existingCoach.LastName != sourceCoach.LastName ||
                                existingCoach.Biography != sourceCoach.Biography ||
                                existingCoach.Photo != sourceCoach.Photo)
                            {
                                existingCoach.FirstName = sourceCoach.FirstName;
                                existingCoach.LastName = sourceCoach.LastName;
                                existingCoach.Biography = sourceCoach.Biography ?? "Default biography";
                                existingCoach.Photo = sourceCoach.Photo;

                                Console.WriteLine($"Updated coach: {sourceCoach.FirstName} {sourceCoach.LastName}");
                            }
                        }
                        else
                        {
                            localContext.Coach.Add(new Coach
                            {
                                CoachId = sourceCoach.CoachId,
                                FirstName = sourceCoach.FirstName,
                                LastName = sourceCoach.LastName,
                                Biography = sourceCoach.Biography ?? "Default biography",
                                Photo = sourceCoach.Photo
                            });

                            Console.WriteLine($"Added new coach: {sourceCoach.FirstName} {sourceCoach.LastName}");
                        }
                    }

                    // Remove coaches that are no longer in the source
                    foreach (var localCoach in localCoaches)
                    {
                        if (!coachesFromSource.Any(c => c.CoachId == localCoach.CoachId))
                        {
                            localContext.Coach.Remove(localCoach);
                            Console.WriteLine($"Removed coach: {localCoach.FirstName} {localCoach.LastName}");
                        }
                    }

                    // Save changes and disable IDENTITY_INSERT
                    await localContext.SaveChangesAsync();
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Coach OFF;");

                    await transaction.CommitAsync();
                    Console.WriteLine("Coach data synchronized successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error syncing coaches: {ex.Message}");
                }
            }
        }

        private static async Task SeedUsersAsync(LocalDbContext localContext)
        {
            var passwordHasher = new PasswordHasher<User>();

            var users = new[]
            {
        new User { Id = 1, UserName = "BigBoy", Email = "bigboy@example.com", IsAdmin = true, SecurityStamp = "5e8f8f5e-4b37-4d4a-b45d-6f6c5a123456" },
        new User { Id = 2, UserName = "JohnSmith", Email = "john@example.com", IsAdmin = false, SecurityStamp = "d9f7a8e3-2f5a-41b1-9836-4d5678912345" },
        new User { Id = 3, UserName = "NoName", Email = "noname@example.com", IsAdmin = false, SecurityStamp = "d452f4d2-1e0a-49bb-9e45-7c6b98765432" },
        new User { Id = 4, UserName = "LuigiMortadella", Email = "luigi@example.com", IsAdmin = false, SecurityStamp = "a2c567d8-8b34-4d23-912c-3f9c23456789" }
    };

            var passwords = new[]
            {
        "safepassword",  // Password for BigBoy
        "123456",        // Password for JohnSmith
        "No Password",   // Password for NoName
        "parmesan"       // Password for LuigiMortadella
    };

            using (var transaction = await localContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Enable IDENTITY_INSERT for the Users table
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users ON;");

                    for (int i = 0; i < users.Length; i++)
                    {
                        // Normalize UserName and Email
                        users[i].NormalizedUserName = users[i].UserName.ToUpper();
                        users[i].NormalizedEmail = users[i].Email.ToUpper();

                        // Hash the password and store it in PasswordHash
                        users[i].PasswordHash = passwordHasher.HashPassword(users[i], passwords[i]);

                        // Add the user to the context
                        localContext.Users.Add(users[i]);
                        Console.WriteLine($"Added user {users[i].UserName}.");
                    }

                    // Save changes to the database
                    await localContext.SaveChangesAsync();

                    // Disable IDENTITY_INSERT for the Users table
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users OFF;");

                    // Commit the transaction
                    await transaction.CommitAsync();
                    Console.WriteLine("Users seeded successfully.");
                }
                catch (Exception ex)
                {
                    // Roll back the transaction in case of an error
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error seeding users: {ex.Message}");
                }
            }
        }
    }
}
