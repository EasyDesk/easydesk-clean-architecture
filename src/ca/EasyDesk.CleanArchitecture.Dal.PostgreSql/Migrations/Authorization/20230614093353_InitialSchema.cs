using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Authorization;

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
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "IdentityRoles",
            schema: "auth",
            columns: table => new
            {
                Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Identity = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                Tenant = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                TenantFk = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IdentityRoles", x => new { x.Identity, x.Role, x.Tenant });
                table.ForeignKey(
                    name: "FK_IdentityRoles_Tenants_TenantFk",
                    column: x => x.TenantFk,
                    principalSchema: "auth",
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            schema: "auth",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PermissionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Tenant = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionName, x.Tenant });
                table.ForeignKey(
                    name: "FK_RolePermissions_Tenants_Tenant",
                    column: x => x.Tenant,
                    principalSchema: "auth",
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IdentityRoles_Tenant",
            schema: "auth",
            table: "IdentityRoles",
            column: "Tenant");

        migrationBuilder.CreateIndex(
            name: "IX_IdentityRoles_TenantFk",
            schema: "auth",
            table: "IdentityRoles",
            column: "TenantFk");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_Tenant",
            schema: "auth",
            table: "RolePermissions",
            column: "Tenant");
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
