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
            using (var coachContext = new Hitdb1Context(serviceProvider.GetRequiredService<DbContextOptions<Hitdb1Context>>()))
            using (var localContext = new LocalDbContext(serviceProvider.GetRequiredService<DbContextOptions<LocalDbContext>>()))
            {
                Console.WriteLine("SeedData: Initializing data...");

                // Ensure local database is created
                await localContext.Database.EnsureCreatedAsync();

                // Fetch coaches from dbo.Coaches in Hitdb1Context (source database)
                var coachesFromSource = await coachContext.Coaches
                    .FromSqlRaw("SELECT CoachId, FirstName, LastName, Biography, Photo FROM dbo.Coaches")
                    .ToListAsync();

                Console.WriteLine($"SeedData: Number of coaches fetched from source: {coachesFromSource.Count}");

                // Fetch coaches from dbo.Coach in LocalDbContext (local database)
                var localCoaches = await localContext.Coach.ToListAsync();
                Console.WriteLine($"SeedData: Number of coaches in local database: {localCoaches.Count}");

                // Begin transaction
                using (var transaction = await localContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Enable IDENTITY_INSERT for dbo.Coach
                        await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Coach ON;");

                        // Update or Add coaches based on source data
                        foreach (var sourceCoach in coachesFromSource)
                        {
                            var existingCoach = localCoaches.FirstOrDefault(c => c.CoachId == sourceCoach.CoachId);

                            if (existingCoach != null)
                            {
                                // Update the coach if details have changed
                                if (existingCoach.FirstName != sourceCoach.FirstName ||
                                    existingCoach.LastName != sourceCoach.LastName ||
                                    existingCoach.Biography != sourceCoach.Biography ||
                                    existingCoach.Photo != sourceCoach.Photo)
                                {
                                    existingCoach.FirstName = sourceCoach.FirstName;
                                    existingCoach.LastName = sourceCoach.LastName;
                                    existingCoach.Biography = sourceCoach.Biography ?? "Default biography";
                                    existingCoach.Photo = sourceCoach.Photo;

                                    Console.WriteLine($"SeedData: Updated coach: {sourceCoach.FirstName} {sourceCoach.LastName}");
                                }
                            }
                            else
                            {
                                // Add the new coach
                                localContext.Coach.Add(new Coach
                                {
                                    CoachId = sourceCoach.CoachId,
                                    FirstName = sourceCoach.FirstName,
                                    LastName = sourceCoach.LastName,
                                    Biography = sourceCoach.Biography ?? "Default biography",
                                    Photo = sourceCoach.Photo
                                });

                                Console.WriteLine($"SeedData: Added new coach: {sourceCoach.FirstName} {sourceCoach.LastName}");
                            }
                        }

                        // Find and remove coaches that exist in the local database but are no longer in the source database
                        foreach (var localCoach in localCoaches)
                        {
                            if (!coachesFromSource.Any(c => c.CoachId == localCoach.CoachId))
                            {
                                localContext.Coach.Remove(localCoach);
                                Console.WriteLine($"SeedData: Removed coach: {localCoach.FirstName} {localCoach.LastName}");
                            }
                        }

                        // Save changes
                        await localContext.SaveChangesAsync();

                        // Disable IDENTITY_INSERT for dbo.Coach
                        await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Coach OFF;");

                        // Commit transaction
                        await transaction.CommitAsync();
                        Console.WriteLine("SeedData: Data synchronized successfully with dbo.Coach.");
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction if there's an error
                        await transaction.RollbackAsync();
                        Console.WriteLine("SeedData: Error occurred: " + ex.Message);
                    }
                }

                // Seed Users if none exist in LocalDbContext
                if (!await localContext.Users.AnyAsync())
                {
                    var passwordHasher = new PasswordHasher<User>();

                    var users = new[]
                    {
                        new User { UserName = "Big Boy", IsAdmin = true },
                        new User { UserName = "John Smith", IsAdmin = false },
                        new User { UserName = "No Name", IsAdmin = false },
                        new User { UserName = "Luigi Mortadella", IsAdmin = false }
                    };

                    // Pre-set passwords (these will be hashed manually during seeding)
                    var passwords = new[]
                    {
                        "safepassword",  // For "Big Boy"
                        "123456",        // For "John Smith"
                        "No Password",   // For "No Name"
                        "parmesan"       // For "Luigi Mortadella"
                    };

                    for (int i = 0; i < users.Length; i++)
                    {
                        users[i].PasswordHash = passwordHasher.HashPassword(users[i], passwords[i]);
                        localContext.Users.Add(users[i]);
                        Console.WriteLine($"SeedData: Added user {users[i].UserName} with pre-set password.");
                    }

                    await localContext.SaveChangesAsync(); // Save changes after adding users
                }

                // Seed Schedule Data if none exist in LocalDbContext
                if (!await localContext.Schedule.AnyAsync())
                {
                    localContext.Schedule.AddRange(
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
                    await localContext.SaveChangesAsync(); // Save changes after adding schedules
                }
            }
        }
    }
}
