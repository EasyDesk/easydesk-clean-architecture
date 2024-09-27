using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auth;

/// <inheritdoc />
public partial class AddApiKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApiKeys",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ApiKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeys", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ApiKeyIdentityModel",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                IdentityId = table.Column<string>(type: "text", nullable: false),
                IdentityRealm = table.Column<string>(type: "text", nullable: false),
                ApiKeyId = table.Column<long>(type: "bigint", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeyIdentityModel", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApiKeyIdentityModel_ApiKeys_ApiKeyId",
                    column: x => x.ApiKeyId,
                    principalSchema: "auth",
                    principalTable: "ApiKeys",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ApiKeyIdentityAttributeModel",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                IdentityId = table.Column<long>(type: "bigint", nullable: false),
                AttributeName = table.Column<string>(type: "text", nullable: false),
                AttributeValue = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeyIdentityAttributeModel", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApiKeyIdentityAttributeModel_ApiKeyIdentityModel_IdentityId",
                    column: x => x.IdentityId,
                    principalSchema: "auth",
                    principalTable: "ApiKeyIdentityModel",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeyIdentityAttributeModel_IdentityId",
            schema: "auth",
            table: "ApiKeyIdentityAttributeModel",
            column: "IdentityId");

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeyIdentityModel_ApiKeyId",
            schema: "auth",
            table: "ApiKeyIdentityModel",
            column: "ApiKeyId");

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeys_ApiKey",
            schema: "auth",
            table: "ApiKeys",
            column: "ApiKey",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApiKeyIdentityAttributeModel",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "ApiKeyIdentityModel",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "ApiKeys",
            schema: "auth");
    }
}
