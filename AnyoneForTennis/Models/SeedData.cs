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
                await CreateIdentityTablesAsync(localContext);

                // Ensure Coaches are seeded first
                await SeedCoachesAsync(coachContext, localContext);

                // Ensure Members are seeded
                await SeedMembersAsync(coachContext, localContext);

                // Fetch schedules from the source database
                var schedulesFromSource = await coachContext.Schedules.ToListAsync();
                Console.WriteLine($"SeedData: Number of schedules fetched from source: {schedulesFromSource.Count}");

                // Fetch schedules from the local database
                var localSchedules = await localContext.Schedule.ToListAsync();
                Console.WriteLine($"SeedData: Number of schedules in local database: {localSchedules.Count}");

                // Sync the schedules data between source and local database
                await SyncSchedules(schedulesFromSource, localSchedules, localContext);

                // Seed predefined schedules if specific ones don't exist
                if (!await localContext.Schedule.AnyAsync(s => s.Name == "Super Tennis Training"
                                                           || s.Name == "Defensive Tennis Drills"
                                                           || s.Name == "Tennis for Beginners"
                                                           || s.Name == "Ultra Marathon Tennis"))
                {
                    // Get coaches for the predefined schedules
                    var coach1 = await localContext.Coach.FirstOrDefaultAsync(c => c.FirstName == "Jane" && c.LastName == "Johnson");
                    var coach2 = await localContext.Coach.FirstOrDefaultAsync(c => c.FirstName == "David" && c.LastName == "Miller");

                    if (coach1 == null || coach2 == null)
                    {
                        Console.WriteLine("Error: Could not find coaches to assign to schedules.");
                        return;
                    }

                    // Seed predefined schedules
                    var predefinedSchedules = new[]
                    {
                new Schedule { Name = "Super Tennis Training", Location = "Court D", Description = "Training for Winners" },
                new Schedule { Name = "Defensive Tennis Drills", Location = "Court A", Description = "Defense is the best Offence" },
                new Schedule { Name = "Tennis for Beginners", Location = "Court C", Description = "Training for Beginners" },
                new Schedule { Name = "Ultra Marathon Tennis", Location = "Court B", Description = "Not for the weak willed" }
            };

                    localContext.Schedule.AddRange(predefinedSchedules);
                    await localContext.SaveChangesAsync();

                    // Assign predefined schedules to coaches with original DateTime values
                    var schedule1 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Super Tennis Training");
                    var schedule2 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Defensive Tennis Drills");
                    var schedule3 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Tennis for Beginners");
                    var schedule4 = await localContext.Schedule.FirstOrDefaultAsync(s => s.Name == "Ultra Marathon Tennis");

                    localContext.SchedulePlus.AddRange(
                        new SchedulePlus { ScheduleId = schedule1.ScheduleId, CoachId = coach1.CoachId, DateTime = new DateTime(2024, 11, 11, 12, 44, 00) },  // Predefined DateTime
                        new SchedulePlus { ScheduleId = schedule2.ScheduleId, CoachId = coach2.CoachId, DateTime = new DateTime(2024, 05, 02, 11, 50, 00) },  // Predefined DateTime
                        new SchedulePlus { ScheduleId = schedule3.ScheduleId, CoachId = coach1.CoachId, DateTime = new DateTime(2024, 10, 09, 12, 55, 55) },  // Predefined DateTime
                        new SchedulePlus { ScheduleId = schedule4.ScheduleId, CoachId = coach2.CoachId, DateTime = new DateTime(2024, 01, 04, 05, 06, 07) }   // Predefined DateTime
                    );
                    await localContext.SaveChangesAsync();
                }

                // Seed users if none exist
                if (!await localContext.Users.AnyAsync())
                {
                    await SeedUsersAsync(localContext);
                }
            }
        }

        // Seed coaches from the source database to the local one
        private static async Task SeedCoachesAsync(Hitdb1Context coachContext, LocalDbContext localContext)
        {
            // Fetch coaches from Hitdb1Context (source database)
            var coachesFromSource = await coachContext.Coaches
                .FromSqlRaw("SELECT CoachId, FirstName, LastName, Biography, Photo FROM dbo.Coaches")
                .ToListAsync();

            // Fetch coaches from the local database
            var localCoaches = await localContext.Coach.ToListAsync();

            // Sync the coaches data between source and local database
            await SyncCoaches(coachesFromSource, localCoaches, localContext);
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
                            // Only update fields if they differ, except for Biography
                            if (existingCoach.FirstName != sourceCoach.FirstName ||
                                existingCoach.LastName != sourceCoach.LastName ||
                                existingCoach.Photo != sourceCoach.Photo ||
                                (existingCoach.Biography == null && sourceCoach.Biography != null)) // Update Biography only if it is null in the local database
                            {
                                existingCoach.FirstName = sourceCoach.FirstName;
                                existingCoach.LastName = sourceCoach.LastName;
                                existingCoach.Photo = sourceCoach.Photo;

                                // Update Biography only if the local Biography is null
                                if (existingCoach.Biography == null)
                                {
                                    existingCoach.Biography = sourceCoach.Biography ?? " ";
                                }

                                Console.WriteLine($"Updated coach: {sourceCoach.FirstName} {sourceCoach.LastName}");
                            }
                        }
                        else
                        {
                            // Add new coach, use the source biography if available
                            localContext.Coach.Add(new Coach
                            {
                                CoachId = sourceCoach.CoachId,
                                FirstName = sourceCoach.FirstName,
                                LastName = sourceCoach.LastName,
                                Biography = sourceCoach.Biography ?? " ",
                                Photo = sourceCoach.Photo
                            });

                            Console.WriteLine($"Added new coach: {sourceCoach.FirstName} {sourceCoach.LastName}");
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


        // Seed members from the source database to the local one
        private static async Task SeedMembersAsync(Hitdb1Context coachContext, LocalDbContext localContext)
        {
            // Fetch members from Hitdb1Context (source database)
            var membersFromSource = await coachContext.Members
                .FromSqlRaw("SELECT MemberId, FirstName, LastName, Email, Active FROM dbo.Members")
                .ToListAsync();

            // Fetch members from the local database
            var localMembers = await localContext.Member.ToListAsync();

            // Sync the members data between source and local database
            await SyncMembers(membersFromSource, localMembers, localContext);
        }

        private static async Task SyncMembers(List<Member> membersFromSource, List<Member> localMembers, LocalDbContext localContext)
        {
            using (var transaction = await localContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Member ON;");

                    foreach (var sourceMember in membersFromSource)
                    {
                        // Check if the member exists in the local database
                        var existingMember = localMembers.FirstOrDefault(m => m.MemberId == sourceMember.MemberId);

                        if (existingMember != null)
                        {
                            // Update the existing member
                            if (existingMember.FirstName != sourceMember.FirstName ||
                                existingMember.LastName != sourceMember.LastName ||
                                existingMember.Email != sourceMember.Email ||
                                existingMember.Active != sourceMember.Active)
                            {
                                existingMember.FirstName = sourceMember.FirstName;
                                existingMember.LastName = sourceMember.LastName;
                                existingMember.Email = sourceMember.Email;
                                existingMember.Active = sourceMember.Active;

                                Console.WriteLine($"Updated member: {sourceMember.FirstName} {sourceMember.LastName}");
                            }
                        }
                        else
                        {
                            // Add new member if it doesn't exist locally
                            localContext.Member.Add(new Member
                            {
                                MemberId = sourceMember.MemberId,
                                FirstName = sourceMember.FirstName,
                                LastName = sourceMember.LastName,
                                Email = sourceMember.Email,
                                Active = sourceMember.Active
                            });

                            Console.WriteLine($"Added new member: {sourceMember.FirstName} {sourceMember.LastName}");
                        }
                    }

                    await localContext.SaveChangesAsync();
                    await localContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Member OFF;");
                    await transaction.CommitAsync();
                    Console.WriteLine("Member data synchronized successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error syncing members: {ex.Message}");
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
        private static async Task SyncSchedules(List<Schedule> schedulesFromSource, List<Schedule> localSchedules, LocalDbContext localContext)
        {
            var random = new Random();

            // List of random tennis-related descriptions
            var tennisDescriptions = new List<string>
            {
                "Master your serve with this focused drill.",
                "Improve your footwork for better court coverage.",
                "Learn advanced forehand techniques from the pros.",
                "Develop your net play with intensive practice.",
                "Get control over your backhand for stronger returns.",
                "Boost your stamina with endurance-based tennis drills.",
                "Train your reflexes with high-speed rallies.",
                "Improve your drop shots with targeted practice.",
                "Master the perfect volley with expert coaching.",
                "Gain consistency in your game with steady practice.",
                "Learn how to slice for precision shots.",
                "Refine your approach shots for aggressive plays.",
                "Dominate your groundstrokes with advanced drills.",
                "Increase your spin control for tricky serves.",
                "Get faster reaction times with rapid-fire drills.",
                "Practice your lob shots for better defensive plays.",
                "Fine-tune your strategy with tactical training.",
                "Enhance your baseline game with power drills.",
                "Develop your doubles game with partner drills.",
                "Get pro tips for better on-court focus."
            };

            using (var transaction = await localContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var sourceSchedule in schedulesFromSource)
                    {
                        // Check if the schedule exists in the local database by its name
                        var existingSchedules = localSchedules.Where(s => s.Name == sourceSchedule.Name).ToList();

                        // Create a new schedule if it's a new entry
                        if (!existingSchedules.Any())
                        {
                            // Create a new schedule if it's not already in the local database
                            var description = tennisDescriptions[random.Next(tennisDescriptions.Count)]; // Assign a random description
                            var newSchedule = new Schedule
                            {
                                Name = sourceSchedule.Name,
                                Location = sourceSchedule.Location,
                                Description = description // Use the random tennis-related description
                            };
                            localContext.Schedule.Add(newSchedule);
                            await localContext.SaveChangesAsync();

                            Console.WriteLine($"Added new schedule: {sourceSchedule.Name}");

                            // Assign SchedulePlus to the new schedule
                            await AddSchedulePlus(localContext, newSchedule.ScheduleId, random);
                        }
                        else
                        {
                            // If the schedule already exists, create a new SchedulePlus one week later
                            var latestSchedule = existingSchedules.OrderByDescending(s => s.ScheduleId).First();
                            Console.WriteLine($"Schedule with name '{sourceSchedule.Name}' already exists. Adding a SchedulePlus one week later.");

                            // Create a new SchedulePlus for the existing schedule
                            await AddSchedulePlus(localContext, latestSchedule.ScheduleId, random, addWeek: true);
                        }
                    }

                    await transaction.CommitAsync();
                    Console.WriteLine("Schedule data synchronized successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error syncing schedules: {ex.Message}");
                }
            }
        }

        // Helper method to add a SchedulePlus for a schedule
        private static async Task AddSchedulePlus(LocalDbContext localContext, int scheduleId, Random random, bool addWeek = false)
        {
            // Get a random coachId (assuming coach IDs are in the range 1 to 20)
            int coachId = random.Next(1, 21);

            // Fetch the most recent SchedulePlus entry for this schedule to determine the new DateTime
            var latestSchedulePlus = await localContext.SchedulePlus
                .Where(sp => sp.ScheduleId == scheduleId)
                .OrderByDescending(sp => sp.DateTime)
                .FirstOrDefaultAsync();

            // Set the DateTime one week after the most recent SchedulePlus or use a random new DateTime
            DateTime newDateTime = latestSchedulePlus != null && addWeek
                ? latestSchedulePlus.DateTime.AddDays(7)
                : new DateTime(2024, random.Next(1, 13), random.Next(1, 29), random.Next(8, 18), random.Next(0, 60), 0); // Random DateTime for new schedule

            // Add the new SchedulePlus
            localContext.SchedulePlus.Add(new SchedulePlus
            {
                ScheduleId = scheduleId,
                CoachId = coachId,
                DateTime = newDateTime
            });
            await localContext.SaveChangesAsync();

            Console.WriteLine($"Added new SchedulePlus for schedule {scheduleId} with DateTime {newDateTime}.");
        }



        private static async Task SeedUsersAsync(LocalDbContext localContext)
        {
            var passwordHasher = new PasswordHasher<User>();

            var users = new[]
            {
        new User { Id = 1, UserName = "BigBoy", Email = "bigboy@example.com", IsAdmin = true, SecurityStamp = "5e8f8f5e-4b37-4d4a-b45d-6f6c5a123456" },
        new User { Id = 2, UserName = "JohnSmith", Email = "john@example.com", IsAdmin = false, SecurityStamp = "d9f7a8e3-2f5a-41b1-9836-4d5678912345" },
        new User { Id = 3, UserName = "NoName", Email = "noname@example.com", IsAdmin = false, SecurityStamp = "d452f4d2-1e0a-49bb-9e45-7c6b98765432" },
        new User { Id = 4, UserName = "LuigiMortadella", Email = "luigi@example.com", IsAdmin = false, SecurityStamp = "a2c567d8-8b34-4d23-912c-3f9c23456789" },
        new User { Id = 5, UserName = "StrobeMan", Email = "strobelight@example.com", IsAdmin = false, SecurityStamp = "c4b12345-8b34-4d23-912c-3f9c23456789" }, // Referencing song 2
        new User { Id = 6, UserName = "GoldRush", Email = "goldrush@example.com", IsAdmin = false, SecurityStamp = "a3b567d8-8b34-4d23-912c-3f9c23456789" },  // Referencing song 7
        new User { Id = 7, UserName = "GunPiece", Email = "gunpiece@example.com", IsAdmin = false, SecurityStamp = "a4c567d8-8b34-4d23-912c-3f9c23456789" }  // Referencing song 8
    };

            var passwords = new[] { "Dragonrage104@", "123456", "No Password", "parmesan", "strobeflash", "golddigger", "gunshot" };

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

                    // Link Luigi's UserId to Emily Smith's CoachId
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

                    // Link new users to members via UserMember table
                    var userMemberLinks = new[]
                    {
                new UserMember { UserId = 5, MemberId = 2 },  // StrobeMan linked to MemberId 2
                new UserMember { UserId = 6, MemberId = 3 },  // GoldRush linked to MemberId 3
                new UserMember { UserId = 7, MemberId = 4 }   // GunPiece linked to MemberId 4
            };

                    localContext.UserMembers.AddRange(userMemberLinks);
                    await localContext.SaveChangesAsync();
                    Console.WriteLine("Linked new users to their respective MemberIds.");

                    await transaction.CommitAsync();
                    Console.WriteLine("Users, User-Coach, and User-Member data seeded successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error seeding users and linking to members/coaches: {ex.Message}");
                }
            }
        }

    }
}
