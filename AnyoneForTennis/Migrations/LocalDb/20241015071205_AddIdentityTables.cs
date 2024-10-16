using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnyoneForTennis.Migrations.LocalDb
{
    public partial class AddIdentityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop foreign keys referencing 'Users'
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.foreign_keys 
                    WHERE parent_object_id = OBJECT_ID('UserCoaches') 
                      AND referenced_object_id = OBJECT_ID('Users')
                )
                BEGIN
                    ALTER TABLE [UserCoaches] DROP CONSTRAINT [FK_UserCoaches_Users_UserId];
                END;

                IF EXISTS (
                    SELECT 1 
                    FROM sys.foreign_keys 
                    WHERE parent_object_id = OBJECT_ID('UserMembers') 
                      AND referenced_object_id = OBJECT_ID('Users')
                )
                BEGIN
                    ALTER TABLE [UserMembers] DROP CONSTRAINT [FK_UserMembers_Users_UserId];
                END;
            ");

            // Step 2: Drop 'Users' table if it exists
            migrationBuilder.Sql(@"
                IF OBJECT_ID('Users', 'U') IS NOT NULL
                BEGIN
                    DROP TABLE Users;
                END;
            ");

            // Step 3: Recreate the 'Users' table with 'Id' as the primary key
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"), // Identity column
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id); // Primary key on 'Id'
                });

            // Step 4: Recreate foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_UserCoaches_Users_UserId",
                table: "UserCoaches",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id", // Updated to reference 'Id'
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMembers_Users_UserId",
                table: "UserMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id", // Updated to reference 'Id'
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop foreign keys referencing 'Users'
            migrationBuilder.DropForeignKey(name: "FK_UserCoaches_Users_UserId", table: "UserCoaches");
            migrationBuilder.DropForeignKey(name: "FK_UserMembers_Users_UserId", table: "UserMembers");

            // Step 2: Drop the 'Users' table
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
