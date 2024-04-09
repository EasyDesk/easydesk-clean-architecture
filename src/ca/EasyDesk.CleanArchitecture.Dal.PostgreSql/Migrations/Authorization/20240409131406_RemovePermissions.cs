using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Authorization;

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
            name: "IX_RolePermissions_Tenant",
            schema: "auth",
            table: "RolePermissions",
            column: "Tenant");
    }
}
