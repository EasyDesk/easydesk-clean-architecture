using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auth;

/// <inheritdoc />
public partial class RemoveMultitenancy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_IdentityRoles_Tenants_TenantFk",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "Tenants",
            schema: "auth");

        migrationBuilder.DropPrimaryKey(
            name: "PK_IdentityRoles",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropIndex(
            name: "IX_IdentityRoles_Tenant",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropIndex(
            name: "IX_IdentityRoles_TenantFk",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropColumn(
            name: "Tenant",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropColumn(
            name: "TenantFk",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.AddPrimaryKey(
            name: "PK_IdentityRoles",
            table: "IdentityRoles",
            columns: new[] { "Identity", "Role", },
            schema: "auth");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_IdentityRoles",
            table: "IdentityRoles",
            schema: "auth");

        migrationBuilder.AddColumn<string>(
            name: "Tenant",
            table: "IdentityRoles",
            type: "character varying(256)",
            maxLength: 256,
            schema: "auth",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "TenantFk",
            table: "IdentityRoles",
            type: "character varying(256)",
            maxLength: 256,
            schema: "auth",
            nullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_IdentityRoles",
            table: "IdentityRoles",
            columns: new[] { "Identity", "Role", "Tenant", },
            schema: "auth");

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
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

        migrationBuilder.AddForeignKey(
            name: "FK_IdentityRoles_Tenants_TenantFk",
            table: "IdentityRoles",
            column: "TenantFk",
            principalTable: "Tenants",
            schema: "auth",
            principalSchema: "auth",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
