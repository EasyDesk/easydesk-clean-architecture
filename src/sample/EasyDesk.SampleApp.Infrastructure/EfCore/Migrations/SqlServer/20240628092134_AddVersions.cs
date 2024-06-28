using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.SqlServer;

/// <inheritdoc />
public partial class AddVersions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "_Version",
            schema: "domain",
            table: "Pets",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<long>(
            name: "_Version",
            schema: "domain",
            table: "People",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "_Version",
            schema: "domain",
            table: "Pets");

        migrationBuilder.DropColumn(
            name: "_Version",
            schema: "domain",
            table: "People");
    }
}
