using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    CoachId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nchar(200)", fixedLength: true, maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "nchar(200)", fixedLength: true, maxLength: 200, nullable: false),
                    Biography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.CoachId);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nchar(200)", fixedLength: true, maxLength: 200, nullable: true),
                    LastName = table.Column<string>(type: "nchar(200)", fixedLength: true, maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nchar(400)", fixedLength: true, maxLength: 400, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberId);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nchar(200)", fixedLength: true, maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nchar(200)", fixedLength: true, maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePlus",
                columns: table => new
                {
                    SchedulePlusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePlus", x => x.SchedulePlusId);
                    table.ForeignKey(
                        name: "FK_SchedulePlus_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "ScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCoaches",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CoachId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoaches", x => new { x.UserId, x.CoachId });
                    table.ForeignKey(
                        name: "FK_UserCoaches_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "CoachId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCoaches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMembers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMembers", x => new { x.UserId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_UserMembers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlus_ScheduleId",
                table: "SchedulePlus",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoaches_CoachId",
                table: "UserCoaches",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMembers_MemberId",
                table: "UserMembers",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulePlus");

            migrationBuilder.DropTable(
                name: "UserCoaches");

            migrationBuilder.DropTable(
                name: "UserMembers");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
