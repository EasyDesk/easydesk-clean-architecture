using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.SqlServer;

/// <inheritdoc />
public partial class AddApprovedField : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "Approved",
            schema: "domain",
            table: "People",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Approved",
            schema: "domain",
            table: "People");
    }
}
