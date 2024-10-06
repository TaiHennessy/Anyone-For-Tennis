﻿// <auto-generated />
using System;
using AnyoneForTennis.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AnyoneForTennis.Migrations
{
    [DbContext(typeof(Hitdb1Context))]
    [Migration("20241006070818_SeededScheduleDataEntity")]
    partial class SeededScheduleDataEntity
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AnyoneForTennis.Models.Coach", b =>
                {
                    b.Property<int>("CoachId")
                        .HasColumnType("int");

                    b.Property<string>("Biography")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nchar(200)")
                        .IsFixedLength();

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nchar(200)")
                        .IsFixedLength();

                    b.Property<byte[]>("Photo")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("CoachId");

                    b.ToTable("Coaches");
                });

            modelBuilder.Entity("AnyoneForTennis.Models.Member", b =>
                {
                    b.Property<int>("MemberId")
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .HasMaxLength(400)
                        .HasColumnType("nchar(400)")
                        .IsFixedLength();

                    b.Property<string>("FirstName")
                        .HasMaxLength(200)
                        .HasColumnType("nchar(200)")
                        .IsFixedLength();

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nchar(200)")
                        .IsFixedLength();

                    b.HasKey("MemberId");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("AnyoneForTennis.Models.Schedule", b =>
                {
                    b.Property<int>("ScheduleId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Location")
                        .HasMaxLength(200)
                        .HasColumnType("nchar(200)")
                        .IsFixedLength();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nchar(200)")
                        .IsFixedLength();

                    b.HasKey("ScheduleId");

                    b.ToTable("Schedules");
                });

            modelBuilder.Entity("AnyoneForTennis.Models.SchedulePlus", b =>
                {
                    b.Property<int>("SchedulePlusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SchedulePlusId"));

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<int>("ScheduleId")
                        .HasColumnType("int");

                    b.HasKey("SchedulePlusId");

                    b.HasIndex("ScheduleId")
                        .IsUnique();

                    b.ToTable("SchedulePlus");
                });

            modelBuilder.Entity("AnyoneForTennis.Models.SchedulePlus", b =>
                {
                    b.HasOne("AnyoneForTennis.Models.Schedule", "Schedule")
                        .WithOne("SchedulePlus")
                        .HasForeignKey("AnyoneForTennis.Models.SchedulePlus", "ScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Schedule");
                });

            modelBuilder.Entity("AnyoneForTennis.Models.Schedule", b =>
                {
                    b.Navigation("SchedulePlus")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
