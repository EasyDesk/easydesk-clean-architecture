using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auth;

/// <inheritdoc />
public partial class RemovePermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RolePermissions",
            schema: "auth");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PermissionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Tenant = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
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
            name: "IX_RolePermissions_Tenant",
            table: "RolePermissions",
            column: "Tenant",
            schema: "auth");
    }
}
