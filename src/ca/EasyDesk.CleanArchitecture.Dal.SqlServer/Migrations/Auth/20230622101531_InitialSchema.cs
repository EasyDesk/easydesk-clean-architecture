using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auth;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "auth");

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "IdentityRoles",
            columns: table => new
            {
                Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Identity = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Realm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TenantFk = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_IdentityRoles", x => new { x.Identity, x.Role, x.Tenant, });
                table.ForeignKey(
                    name: "FK_IdentityRoles_Tenants_TenantFk",
                    column: x => x.TenantFk,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    principalSchema: "auth",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionName, x.Tenant, });
                table.ForeignKey(
                    name: "FK_RolePermissions_Tenants_Tenant",
                    column: x => x.Tenant,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    principalSchema: "auth",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IdentityRoles_Tenant",
            table: "IdentityRoles",
            column: "Tenant",
            schema: "auth");

        migrationBuilder.CreateIndex(
            name: "IX_IdentityRoles_TenantFk",
            table: "IdentityRoles",
            column: "TenantFk",
            schema: "auth");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_Tenant",
            table: "RolePermissions",
            column: "Tenant",
            schema: "auth");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "RolePermissions",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "Tenants",
            schema: "auth");
    }
}
