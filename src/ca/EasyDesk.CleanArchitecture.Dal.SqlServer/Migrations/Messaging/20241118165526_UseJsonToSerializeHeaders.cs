using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Messaging;

/// <inheritdoc />
public partial class UseJsonToSerializeHeaders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Headers",
            table: "Outbox",
            newName: "Headers_Old",
            schema: "messaging");

        migrationBuilder.AddColumn<string>(
            name: "Headers",
            table: "Outbox",
            type: "nvarchar(max)",
            schema: "messaging",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Headers",
            table: "Outbox",
            schema: "messaging");

        migrationBuilder.RenameColumn(
            name: "Headers_Old",
            table: "Outbox",
            newName: "Headers",
            schema: "messaging");
    }
}
