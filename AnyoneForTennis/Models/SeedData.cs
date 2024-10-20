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

                // Create the Identity tables
                await CreateIdentityTablesAsync(localContext);

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

                    // Seed schedules
                    var seededSchedules = new[]
                    {
                        new Schedule { Name = "Super Tennis Training", Location = "Court D", Description = "Training for Winners" },
                        new Schedule { Name = "Defensive Tennis Drills", Location = "Court A", Description = "Defense is the best Offence" },
                        new Schedule { Name = "Tennis for Beginners", Location = "Court C", Description = "Training for Beginners" },
                        new Schedule { Name = "Ultra Marathon Tennis", Location = "Court B", Description = "Not for the weak willed" }
                    };

                    localContext.Schedule.AddRange(seededSchedules);
                    await localContext.SaveChangesAsync();

                    var schedule1 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Super Tennis Training");
                    var schedule2 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Defensive Tennis Drills");
                    var schedule3 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Tennis for Beginners");
                    var schedule4 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Ultra Marathon Tennis");

                    localContext.SchedulePlus.AddRange(
                        new SchedulePlus { ScheduleId = schedule1.ScheduleId, CoachId = coach1.CoachId, DateTime = new DateTime(2024, 11, 11, 12, 44, 00) },
                        new SchedulePlus { ScheduleId = schedule2.ScheduleId, CoachId = coach2.CoachId, DateTime = new DateTime(2024, 05, 02, 11, 50, 00) },
                        new SchedulePlus { ScheduleId = schedule3.ScheduleId, CoachId = coach1.CoachId, DateTime = new DateTime(2024, 10, 09, 12, 55, 55) },
                        new SchedulePlus { ScheduleId = schedule4.ScheduleId, CoachId = coach2.CoachId, DateTime = new DateTime(2024, 01, 04, 05, 06, 07) }
                    );
                    await localContext.SaveChangesAsync();
                }
            }
        }

        private static async Task CreateIdentityTablesAsync(LocalDbContext context)
        {
            // Create AspNetRoles Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NULL
                CREATE TABLE AspNetRoles (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(256) NULL,
                    NormalizedName NVARCHAR(256) NULL UNIQUE,
                    ConcurrencyStamp NVARCHAR(MAX) NULL
                );
            ");

            // Create AspNetUsers Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
                CREATE TABLE AspNetUsers (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserName NVARCHAR(256) NULL,
                    NormalizedUserName NVARCHAR(256) NULL UNIQUE,
                    Email NVARCHAR(256) NULL,
                    NormalizedEmail NVARCHAR(256) NULL,
                    EmailConfirmed BIT NOT NULL DEFAULT 0,
                    PasswordHash NVARCHAR(MAX) NULL,
                    SecurityStamp NVARCHAR(MAX) NULL,
                    ConcurrencyStamp NVARCHAR(MAX) NULL,
                    PhoneNumber NVARCHAR(MAX) NULL,
                    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
                    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
                    LockoutEnd DATETIMEOFFSET NULL,
                    LockoutEnabled BIT NOT NULL DEFAULT 0,
                    AccessFailedCount INT NOT NULL DEFAULT 0
                );
            ");

            // Create AspNetUserClaims Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NULL
                CREATE TABLE AspNetUserClaims (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId INT NOT NULL,
                    ClaimType NVARCHAR(MAX) NULL,
                    ClaimValue NVARCHAR(MAX) NULL,
                    CONSTRAINT FK_AspNetUserClaims_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
                );
            ");

            // Create AspNetUserLogins Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NULL
                CREATE TABLE AspNetUserLogins (
                    LoginProvider NVARCHAR(450) NOT NULL,
                    ProviderKey NVARCHAR(450) NOT NULL,
                    ProviderDisplayName NVARCHAR(MAX) NULL,
                    UserId INT NOT NULL,
                    PRIMARY KEY (LoginProvider, ProviderKey),
                    CONSTRAINT FK_AspNetUserLogins_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
                );
            ");

            // Create AspNetUserRoles Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NULL
                CREATE TABLE AspNetUserRoles (
                    UserId INT NOT NULL,
                    RoleId INT NOT NULL,
                    PRIMARY KEY (UserId, RoleId),
                    CONSTRAINT FK_AspNetUserRoles_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_AspNetUserRoles_Role FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
                );
            ");

            // Create AspNetUserTokens Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NULL
                CREATE TABLE AspNetUserTokens (
                    UserId INT NOT NULL,
                    LoginProvider NVARCHAR(450) NOT NULL,
                    Name NVARCHAR(450) NOT NULL,
                    Value NVARCHAR(MAX) NULL,
                    PRIMARY KEY (UserId, LoginProvider, Name),
                    CONSTRAINT FK_AspNetUserTokens_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
                );
            ");

            // Create AspNetRoleClaims Table
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NULL
                CREATE TABLE AspNetRoleClaims (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    RoleId INT NOT NULL,
                    ClaimType NVARCHAR(MAX) NULL,
                    ClaimValue NVARCHAR(MAX) NULL,
                    CONSTRAINT FK_AspNetRoleClaims_Role FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
                );
            ");
        }

        private static async Task SyncCoaches(List<Coach> coachesFromSource, List<Coach> localCoaches, LocalDbContext localContext)
        {
            using (var transaction = await localContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Coach ON;");

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

                    foreach (var localCoach in localCoaches)
                    {
                        if (!coachesFromSource.Any(c => c.CoachId == localCoach.CoachId))
                        {
                            localContext.Coach.Remove(localCoach);
                            Console.WriteLine($"Removed coach: {localCoach.FirstName} {localCoach.LastName}");
                        }
                    }

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

            var passwords = new[] { "safepassword", "123456", "No Password", "parmesan" };

            using (var transaction = await localContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users ON;");

                    for (int i = 0; i < users.Length; i++)
                    {
                        users[i].NormalizedUserName = users[i].UserName.ToUpper();
                        users[i].NormalizedEmail = users[i].Email.ToUpper();
                        users[i].PasswordHash = passwordHasher.HashPassword(users[i], passwords[i]);
                        localContext.Users.Add(users[i]);
                        Console.WriteLine($"Added user {users[i].UserName}.");
                    }

                    await localContext.SaveChangesAsync();
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users OFF;");

                    var luigiUserId = users.First(u => u.UserName == "LuigiMortadella").Id;
                    var userCoach = await localContext.UserCoaches.FirstOrDefaultAsync(uc => uc.UserId == 4 && uc.CoachId == 2);

                    if (userCoach == null)
                    {
                        localContext.UserCoaches.Add(new UserCoach
                        {
                            UserId = 4,
                            CoachId = 2
                        });

                        await localContext.SaveChangesAsync();
                        Console.WriteLine("Linked Luigi's UserId (4) to Emily Smith's CoachId (2).");
                    }
                    else
                    {
                        Console.WriteLine("Luigi's UserId is already linked to Emily Smith's CoachId (2).");
                    }

                    await transaction.CommitAsync();
                    Console.WriteLine("Users and User-Coach data seeded successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error seeding users and linking to coaches: {ex.Message}");
                }
            }
        }
    }
}
