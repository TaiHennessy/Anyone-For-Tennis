using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations.LocalDb
{
    /// <inheritdoc />
    public partial class fifthary9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus");

            migrationBuilder.DropIndex(
                name: "IX_SchedulePlus_CoachId",
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

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlus_CoachId",
                table: "SchedulePlus",
                column: "CoachId",
                unique: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "CoachId",
                table: "SchedulePlus",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlus_CoachId",
                table: "SchedulePlus",
                column: "CoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchedulePlus_Coach_CoachId",
                table: "SchedulePlus",
                column: "CoachId",
                principalTable: "Coach",
                principalColumn: "CoachId");
        }
    }
}
