using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auth;

/// <inheritdoc />
public partial class AddApiKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApiKeys",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ApiKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            },
            schema: "auth",
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeys", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ApiKeyIdentityModel",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                IdentityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IdentityRealm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ApiKeyId = table.Column<long>(type: "bigint", nullable: false),
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
            name: "ApiKeyIdentityAttributeModel",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                IdentityId = table.Column<long>(type: "bigint", nullable: false),
                AttributeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AttributeValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
