using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations
{
    /// <inheritdoc />
    public partial class SeededScheduleDataEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlus_ScheduleId",
                table: "SchedulePlus",
                column: "ScheduleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulePlus");
        }
    }
}
