using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Messaging;

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
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                Headers = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                DestinationAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
