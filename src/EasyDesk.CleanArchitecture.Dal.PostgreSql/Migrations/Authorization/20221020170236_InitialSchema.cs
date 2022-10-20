using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Authorization;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "auth");

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            schema: "auth",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PermissionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionName });
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            schema: "auth",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RolePermissions",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "UserRoles",
            schema: "auth");
    }
}
