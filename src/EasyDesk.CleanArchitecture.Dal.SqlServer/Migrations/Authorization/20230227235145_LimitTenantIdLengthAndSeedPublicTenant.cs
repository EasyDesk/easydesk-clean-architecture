using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Authorization;

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
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

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
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(256)",
            oldMaxLength: 256);
    }
}
