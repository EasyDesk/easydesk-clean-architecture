using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.Sqlite.Migrations.Auth;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "auth");

        migrationBuilder.CreateTable(
            name: "ApiKeys",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ApiKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeys", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ApiKeyIdentityModel",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                IdentityId = table.Column<string>(type: "TEXT", nullable: false),
                IdentityRealm = table.Column<string>(type: "TEXT", nullable: false),
                ApiKeyId = table.Column<long>(type: "INTEGER", nullable: false)
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeyIdentityModel", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApiKeyIdentityModel_ApiKeys_ApiKeyId",
                    column: x => x.ApiKeyId,
                    principalTable: "ApiKeys",
                    principalColumn: "Id",
                    principalSchema: "auth",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "IdentityRoles",
            columns: table => new
            {
                Role = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Identity = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                Tenant = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Realm = table.Column<string>(type: "TEXT", nullable: false),
                TenantFk = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_IdentityRoles", x => new { x.Identity, x.Role, x.Tenant });
                table.ForeignKey(
                    name: "FK_IdentityRoles_Tenants_TenantFk",
                    column: x => x.TenantFk,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    principalSchema: "auth",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ApiKeyIdentityAttributeModel",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                IdentityId = table.Column<long>(type: "INTEGER", nullable: false),
                AttributeName = table.Column<string>(type: "TEXT", nullable: false),
                AttributeValue = table.Column<string>(type: "TEXT", nullable: false)
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeyIdentityAttributeModel", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApiKeyIdentityAttributeModel_ApiKeyIdentityModel_IdentityId",
                    column: x => x.IdentityId,
                    principalTable: "ApiKeyIdentityModel",
                    principalColumn: "Id",
                    principalSchema: "auth",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeyIdentityAttributeModel_IdentityId",
            table: "ApiKeyIdentityAttributeModel",
            column: "IdentityId",
            schema: "auth");

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeyIdentityModel_ApiKeyId",
            table: "ApiKeyIdentityModel",
            column: "ApiKeyId",
            schema: "auth");

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeys_ApiKey",
            table: "ApiKeys",
            column: "ApiKey",
            schema: "auth",
            unique: true);

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
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApiKeyIdentityAttributeModel",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "IdentityRoles",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "ApiKeyIdentityModel",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "Tenants",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "ApiKeys",
            schema: "auth");
    }
}
