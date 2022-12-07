using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Authorization;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "auth");

        migrationBuilder.CreateTable(
            name: "Tenants",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            schema: "auth",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionName });
                table.ForeignKey(
                    name: "FK_RolePermissions_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalSchema: "auth",
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            schema: "auth",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_UserRoles_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalSchema: "auth",
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_TenantId",
            schema: "auth",
            table: "RolePermissions",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_TenantId",
            schema: "auth",
            table: "UserRoles",
            column: "TenantId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RolePermissions",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "UserRoles",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "Tenants",
            schema: "auth");
    }
}
