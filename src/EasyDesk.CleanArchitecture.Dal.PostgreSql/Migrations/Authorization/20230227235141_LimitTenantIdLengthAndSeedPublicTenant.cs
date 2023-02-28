using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Authorization;

/// <inheritdoc />
public partial class LimitTenantIdLengthAndSeedPublicTenant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Id",
            schema: "auth",
            table: "Tenants",
            type: "character varying(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.InsertData(
            schema: "auth",
            table: "Tenants",
            column: "Id",
            value: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "auth",
            table: "Tenants",
            keyColumn: "Id",
            keyValue: string.Empty);

        migrationBuilder.AlterColumn<string>(
            name: "Id",
            schema: "auth",
            table: "Tenants",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(256)",
            oldMaxLength: 256);
    }
}
