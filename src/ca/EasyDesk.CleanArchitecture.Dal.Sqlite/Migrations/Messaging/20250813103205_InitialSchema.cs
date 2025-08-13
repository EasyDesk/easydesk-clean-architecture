using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.Sqlite.Migrations.Messaging;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "messaging");

        migrationBuilder.CreateTable(
            name: "Inbox",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                Instant = table.Column<string>(type: "TEXT", nullable: false)
            },
            schema: "messaging",
            constraints: table =>
            {
                table.PrimaryKey("PK_Inbox", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Outbox",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Content = table.Column<byte[]>(type: "BLOB", nullable: false),
                Headers = table.Column<string>(type: "TEXT", nullable: false),
                Headers_Old = table.Column<byte[]>(type: "BLOB", nullable: false),
                DestinationAddress = table.Column<string>(type: "TEXT", nullable: false)
            },
            schema: "messaging",
            constraints: table =>
            {
                table.PrimaryKey("PK_Outbox", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Inbox",
            schema: "messaging");

        migrationBuilder.DropTable(
            name: "Outbox",
            schema: "messaging");
    }
}
