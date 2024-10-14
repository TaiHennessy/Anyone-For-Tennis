using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations.LocalDb
{
    /// <inheritdoc />
    public partial class CoachIdRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId1",
                table: "SchedulePlus");

            migrationBuilder.DropIndex(
                name: "IX_SchedulePlus_CoachId1",
                table: "SchedulePlus");

            migrationBuilder.DropColumn(
                name: "CoachId1",
                table: "SchedulePlus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoachId1",
                table: "SchedulePlus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlus_CoachId1",
                table: "SchedulePlus",
                column: "CoachId1",
                unique: true,
                filter: "[CoachId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId1",
                table: "SchedulePlus",
                column: "CoachId1",
                principalTable: "Coach",
                principalColumn: "CoachId");
        }
    }
}
