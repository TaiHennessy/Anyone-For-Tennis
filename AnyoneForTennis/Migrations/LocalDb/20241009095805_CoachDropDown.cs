using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations.LocalDb
{
    /// <inheritdoc />
    public partial class CoachDropDown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoachId",
                table: "SchedulePlus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlus_CoachId",
                table: "SchedulePlus",
                column: "CoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus",
                column: "CoachId",
                principalTable: "Coach",
                principalColumn: "CoachId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus");

            migrationBuilder.DropIndex(
                name: "IX_SchedulePlus_CoachId",
                table: "SchedulePlus");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "SchedulePlus");
        }
    }
}
