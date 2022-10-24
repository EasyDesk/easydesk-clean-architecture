using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Migrations;

public partial class AddDateOfBirth : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Married",
            schema: "domain",
            table: "People");

        migrationBuilder.RenameColumn(
            name: "Name",
            schema: "domain",
            table: "People",
            newName: "LastName");

        migrationBuilder.AddColumn<LocalDate>(
            name: "DateOfBirth",
            schema: "domain",
            table: "People",
            type: "date",
            nullable: false,
            defaultValue: new NodaTime.LocalDate(1, 1, 1));

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            schema: "domain",
            table: "People",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DateOfBirth",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "FirstName",
            schema: "domain",
            table: "People");

        migrationBuilder.RenameColumn(
            name: "LastName",
            schema: "domain",
            table: "People",
            newName: "Name");

        migrationBuilder.AddColumn<bool>(
            name: "Married",
            schema: "domain",
            table: "People",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }
}
