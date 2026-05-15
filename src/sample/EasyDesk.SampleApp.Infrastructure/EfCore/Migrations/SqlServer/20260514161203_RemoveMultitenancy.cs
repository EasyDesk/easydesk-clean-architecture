using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.SqlServer;

/// <inheritdoc />
public partial class RemoveMultitenancy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Pets_Tenant",
            table: "Pets",
            schema: "domain");

        migrationBuilder.DropIndex(
            name: "IX_People_Tenant",
            table: "People",
            schema: "domain");

        migrationBuilder.DropColumn(
            name: "Tenant",
            table: "Pets",
            schema: "domain");

        migrationBuilder.DropColumn(
            name: "Tenant",
            table: "People",
            schema: "domain");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Tenant",
            table: "Pets",
            type: "nvarchar(256)",
            maxLength: 256,
            schema: "domain",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Tenant",
            table: "People",
            type: "nvarchar(256)",
            maxLength: 256,
            schema: "domain",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_Pets_Tenant",
            table: "Pets",
            column: "Tenant",
            schema: "domain");

        migrationBuilder.CreateIndex(
            name: "IX_People_Tenant",
            table: "People",
            column: "Tenant",
            schema: "domain");
    }
}
