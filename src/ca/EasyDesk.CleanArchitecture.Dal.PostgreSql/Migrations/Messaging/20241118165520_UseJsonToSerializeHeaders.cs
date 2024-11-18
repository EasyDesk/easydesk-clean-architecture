using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Messaging;

/// <inheritdoc />
public partial class UseJsonToSerializeHeaders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Headers",
            schema: "messaging",
            table: "Outbox",
            newName: "Headers_Old");

        migrationBuilder.AddColumn<string>(
            name: "Headers",
            schema: "messaging",
            table: "Outbox",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Headers",
            schema: "messaging",
            table: "Outbox");

        migrationBuilder.RenameColumn(
            name: "Headers_Old",
            schema: "messaging",
            table: "Outbox",
            newName: "Headers");
    }
}
