using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations.LocalDb
{
    /// <inheritdoc />
    public partial class CoachesInScheculePlus2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus");

            migrationBuilder.AlterColumn<int>(
                name: "CoachId",
                table: "SchedulePlus",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus",
                column: "CoachId",
                principalTable: "Coach",
                principalColumn: "CoachId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus");

            migrationBuilder.AlterColumn<int>(
                name: "CoachId",
                table: "SchedulePlus",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus",
                column: "CoachId",
                principalTable: "Coach",
                principalColumn: "CoachId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
