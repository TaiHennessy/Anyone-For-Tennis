using System;
using System.Collections.Generic;
using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnyoneForTennis.Data
{
    public partial class Hitdb1Context : DbContext
    {
        public Hitdb1Context()
        {
        }

        public Hitdb1Context(DbContextOptions<Hitdb1Context> options) : base(options)
        {
        }

        public virtual DbSet<Coach> Coaches { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect sensitive information in your connection string, move it out of source code.
            => optionsBuilder.UseSqlServer(
                "data source=scithitdb1.cducloud.cdu.edu.au;initial catalog=HITDB1;" +
                "user id=Student1Login;password=g85{K4OzKF3<;encrypt=optional");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coach>(entity =>
            {
                entity.HasKey(e => e.CoachId);
                entity.Property(e => e.CoachId).ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Biography).HasMaxLength(200);
                entity.Property(e => e.Photo).HasMaxLength(200);
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.MemberId);
                entity.Property(e => e.MemberId).ValueGeneratedOnAdd();
                entity.Property(e => e.Email).HasMaxLength(400);
                entity.Property(e => e.FirstName).HasMaxLength(200);
                entity.Property(e => e.LastName).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleId);
                entity.Property(e => e.ScheduleId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public class LocalDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public DbSet<UserMember> UserMembers { get; set; }
        public DbSet<UserCoach> UserCoaches { get; set; }
        public DbSet<SchedulePlus> SchedulePlus { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Coach> Coach { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure Identity-related tables are created

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.IsAdmin).IsRequired();
            });

            modelBuilder.Entity<UserMember>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.MemberId });
                entity.HasOne(um => um.User)
                    .WithMany(u => u.UserMembers)
                    .HasForeignKey(um => um.UserId);
                entity.HasOne(um => um.Member)
                    .WithMany()
                    .HasForeignKey(um => um.MemberId);
            });

            modelBuilder.Entity<UserCoach>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CoachId });
                entity.HasOne(uc => uc.User)
                    .WithMany(u => u.UserCoaches)  // This is the navigation property on User
                    .HasForeignKey(uc => uc.UserId);
                entity.HasOne(uc => uc.Coach)
                    .WithMany()
                    .HasForeignKey(uc => uc.CoachId);
            });


            modelBuilder.Entity<SchedulePlus>(entity =>
            {
                entity.HasKey(e => e.SchedulePlusId);
                entity.Property(e => e.DateTime).IsRequired();
                entity.Property(e => e.Duration).IsRequired();

                entity.HasOne(sp => sp.Schedule)
                    .WithOne(s => s.SchedulePlus)
                    .HasForeignKey<SchedulePlus>(sp => sp.ScheduleId);

                entity.HasOne(sp => sp.Coach)
                    .WithMany(c => c.SchedulePlusPlus)
                    .HasForeignKey(sp => sp.CoachId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Coach>(entity =>
            {
                entity.HasKey(e => e.CoachId);
                entity.Property(e => e.FirstName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Biography).HasMaxLength(200);
                entity.Property(e => e.Photo).HasMaxLength(200);
            });
        }
    }
}
