using System;
using System.Collections.Generic;
using AnyoneForTennis.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyoneForTennis.Data;

public partial class Hitdb1Context : DbContext
{
    public Hitdb1Context()
    {
    }

    public Hitdb1Context(DbContextOptions<Hitdb1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Coach> Coaches { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("data source=scithitdb1.cducloud.cdu.edu.au;initial catalog=HITDB1;user id=Student1Login;password=g85{K4OzKF3<;encrypt=optional");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coach>(entity =>
        {
            entity.Property(e => e.CoachId).ValueGeneratedNever();
            entity.Property(e => e.FirstName)
                .HasMaxLength(200)
                .IsFixedLength();
            entity.Property(e => e.LastName)
                .HasMaxLength(200)
                .IsFixedLength();
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.Property(e => e.MemberId).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(400)
                .IsFixedLength();
            entity.Property(e => e.FirstName)
                .HasMaxLength(200)
                .IsFixedLength();
            entity.Property(e => e.LastName)
                .HasMaxLength(200)
                .IsFixedLength();
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.ScheduleId).ValueGeneratedNever();
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

public class LocalDbContext : DbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options)
        : base(options)
    {
    }

    // Define local-only tables
    public DbSet<User> Users { get; set; }
    public DbSet<UserMember> UserMembers { get; set; }
    public DbSet<UserCoach> UserCoaches { get; set; }
    public DbSet<SchedulePlus> SchedulePlus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure local-only models
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Password).HasMaxLength(200).IsRequired();
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
                .WithMany(u => u.UserCoaches)
                .HasForeignKey(uc => uc.UserId);
            entity.HasOne(uc => uc.Coach)
                .WithMany()
                .HasForeignKey(uc => uc.CoachId);
        });

        modelBuilder.Entity<SchedulePlus>(entity =>
        {
            entity.HasKey(e => e.SchedulePlusId);
            entity.HasOne(sp => sp.Schedule)
                .WithMany()
                .HasForeignKey(sp => sp.ScheduleId);
            entity.Property(sp => sp.DateTime).IsRequired();
            entity.Property(sp => sp.Duration).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
