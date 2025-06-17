using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.PostgreSql;

/// <inheritdoc />
public partial class AddVersions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "_Version",
            table: "Pets",
            type: "bigint",
            schema: "domain",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<long>(
            name: "_Version",
            table: "People",
            type: "bigint",
            schema: "domain",
            nullable: false,
            defaultValue: 0L);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "_Version",
            table: "Pets",
            schema: "domain");

        migrationBuilder.DropColumn(
            name: "_Version",
            table: "People",
            schema: "domain");
    }
}
