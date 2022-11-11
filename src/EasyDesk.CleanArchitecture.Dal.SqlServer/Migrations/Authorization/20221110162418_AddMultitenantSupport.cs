using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Authorization;

public partial class AddMultitenantSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TenantId",
            schema: "auth",
            table: "UserRoles",
            type: "nvarchar(450)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TenantId",
            schema: "auth",
            table: "RolePermissions",
            type: "nvarchar(450)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_TenantId",
            schema: "auth",
            table: "UserRoles",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_TenantId",
            schema: "auth",
            table: "RolePermissions",
            column: "TenantId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserRoles_TenantId",
            schema: "auth",
            table: "UserRoles");

        migrationBuilder.DropIndex(
            name: "IX_RolePermissions_TenantId",
            schema: "auth",
            table: "RolePermissions");

        migrationBuilder.DropColumn(
            name: "TenantId",
            schema: "auth",
            table: "UserRoles");

        migrationBuilder.DropColumn(
            name: "TenantId",
            schema: "auth",
            table: "RolePermissions");
    }
}
